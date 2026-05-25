using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.DTO.Paymob;
using Project.Core.DTOs.Paymob;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using Project.Core.Settings;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public class EventPaymentService : IEventPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobEventSettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBookingService _bookingService;
        private readonly IConfiguration _configuration;
        public EventPaymentService(
            HttpClient httpClient,
            IOptions<PaymobEventSettings> settings,
            IUnitOfWork unitOfWork,
            IEventBookingService bookingService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _bookingService = bookingService;
            _configuration = configuration;
        }

        // ==========================================
        // 1. الدالة الرئيسية لبدء الدفع
        // ==========================================
        public async Task<string> InitiateEventPaymentAsync(InitiateEventPaymentDto dto)
        {
            // ✅ نجيب الـ Booking الأول ومنه نجيب الـ EventId الصح
            var booking = await _unitOfWork.EventBookings.GetByIdAsync(dto.BookingId);
            if (booking == null) throw new Exception("الحجز غير موجود");

            if (booking.Status != BookingStatus.Pending)
                throw new Exception("هذا الحجز غير متاح للدفع");

            var eventEntity = await _unitOfWork.Events.GetByIdAsync(booking.EventId);
            if (eventEntity == null) throw new Exception("الحدث غير موجود");

            decimal amountToPay = eventEntity.Price;
            var authToken = await GetAuthToken();

            string amountCents = (amountToPay * 100).ToString();
            var paymobOrderId = await RegisterOrder(authToken, amountCents);

            var paymentKey = await GetPaymentKey(authToken, paymobOrderId.ToString(), amountCents, _settings.CardIntegrationId);

            var eventPayment = new EventPayment
            {
                Id = Guid.NewGuid(),
                Amount = amountToPay,
                Currency = "EGP",
                PaymentMethod = PaymentMethod.CreditCard,
                Status = PaymentStatus.Pending,
                TransactionDate = DateTime.UtcNow,
                PaymobOrderId = paymobOrderId,
                UserId = dto.UserId,
                EventId = booking.EventId // ✅ من الـ Booking مش من الـ dto
            };

            await _unitOfWork.EventPayments.AddAsync(eventPayment);
            await _unitOfWork.SaveChangesAsync();

            return $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentKey}";
        }

        // ==========================================
        // 2. دالة استقبال الـ Webhook (الرد من بايموب)
        // ==========================================
        public async Task ProcessEventWebhookAsync(PaymobWebhookDto dto)
        {
            if (!ValidateHmac(dto))
                throw new Exception("Invalid HMAC signature");

            var transaction = dto.Obj;
            long paymobOrderId = transaction.Order.Id;

            // استخدام الريبوزيتري الجديد الخاص بالإيفينتات
            var payment = await _unitOfWork.EventPayments.GetByPaymobOrderIdAsync(paymobOrderId);

            if (payment == null || payment.Status != PaymentStatus.Pending)
                return;

            payment.PaymobTransactionId = transaction.Id;
            payment.TransactionDate = DateTime.UtcNow;

            if (transaction.Success)
            {
                payment.Status = PaymentStatus.Completed;
                await ConfirmEventBookingAsync(payment); // استدعاء دالة تأكيد التذكرة
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ConfirmEventBookingAsync(EventPayment payment)
        {
            if (payment.UserId == Guid.Empty || payment.EventId == Guid.Empty)
                return;

            var bookingDto = await _bookingService.GetUserBookingForEventAsync(payment.UserId, payment.EventId);

            // ✅ قبلنا Pending و Cancelled (ممكن Hangfire لغاه قبل الـ Webhook)
            if (bookingDto != null &&
                (bookingDto.Status == BookingStatus.Pending.ToString() ||
                 bookingDto.Status == BookingStatus.Cancelled.ToString()))
            {
                var confirmDto = new ConfirmPaymentDto
                {
                    BookingId = bookingDto.Id,
                    TransactionId = payment.PaymobTransactionId?.ToString() ?? "Paymob-Webhook"
                };

                await _bookingService.ConfirmPaymentAsync(Guid.Empty, confirmDto);
            }
        }

        // ==========================================
        // 3. الدوال المساعدة (Helper Methods)
        // ==========================================
        private async Task<string> GetAuthToken()
        {
            var request = new PaymobAuthRequest { ApiKey = _settings.ApiKey };
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", request);

            // لو الريكويست فشل (زي حالة الـ 400 كده)
            if (!response.IsSuccessStatusCode)
            {
                // هنقرا رسالة الخطأ اللي بيموب باعتها
                var errorDetails = await response.Content.ReadAsStringAsync();

                // هنضرب إكسبشن بيعرض رسالة بيموب + بيعرض الـ ApiKey اللي الكود قراه عشان نتأكد إنه مش فاضي
                throw new Exception($"Paymob Error: {errorDetails} | ApiKey Used: {_settings.ApiKey}");
            }

            var result = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>();
            return result!.Token;
        }

        private async Task<long> RegisterOrder(string authToken, string amountCents)
        {
            var request = new PaymobOrderRequest
            {
                AuthToken = authToken,
                AmountCents = amountCents,
                DeliveryNeeded = "false",
                Items = new List<object>()
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", request);
            // 👇 فخ اصطياد الإيرور
            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Paymob Order Error: {errorDetails}");
            }

            var result = await response.Content.ReadFromJsonAsync<PaymobOrderResponse>();
            return result!.Id;
        }
        

        // غيرنا نوع integrationId لـ int
        private async Task<string> GetPaymentKey(string authToken, string orderId, string amountCents, int integrationId)
        {
            var request = new PaymobKeyRequest
            {
                AuthToken = authToken,
                AmountCents = amountCents,
                Expiration = 3600,
                OrderId = orderId,
                BillingData = new PaymobBillingData
                {
                    Country = "EG",
                    City = "Cairo",
                    FirstName = "Event",
                    LastName = "User",
                    PhoneNumber = "+201012345678",
                    Email = "user@test.com",
                },
                Currency = "EGP",
                IntegrationId = integrationId // 👈 دلوقتي هتقبل الـ int عادي جداً
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobKeyResponse>();
            return result!.Token;
        }

        private bool ValidateHmac(PaymobWebhookDto dto)
        {
            var data = dto.Obj;

            string concatenatedString =
                data.AmountCents.ToString() +
                data.CreatedAt +
                data.Currency +
                (data.ErrorOccured ? "true" : "false") +
                (data.HasParentTransaction ? "true" : "false") +
                data.Id.ToString() +
                data.IntegrationId.ToString() +
                (data.Is3dSecure ? "true" : "false") +
                (data.IsAuth ? "true" : "false") +
                (data.IsCapture ? "true" : "false") +
                (data.IsRefunded ? "true" : "false") +
                (data.IsStandalonePayment ? "true" : "false") +
                (data.IsVoided ? "true" : "false") +
                data.Order.Id.ToString() +
                data.Owner.ToString() +
                (data.Pending ? "true" : "false") +
                data.SourceData.Pan +
                data.SourceData.SubType +
                data.SourceData.Type +
                (data.Success ? "true" : "false");

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HmacSecret.Trim()));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));
            var calculatedHmac = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return calculatedHmac.Equals(dto.Hmac, StringComparison.OrdinalIgnoreCase);
        }
        public void SomeMethod()
        {
            // كدا إنت بتقرأ القيمة اللي إنت لسه ضايفها في Azure حالا!
            var ApiKey = _configuration["PaymobEvent:ApiKey"];
            var hmac = _configuration["PaymobEvent:HmacSecret"];
            var integrationId = _configuration["PaymobEvent:IntegrationId"];           
            var IframeId = _configuration["PaymobEvent:IframeId"];
        }
    }
}