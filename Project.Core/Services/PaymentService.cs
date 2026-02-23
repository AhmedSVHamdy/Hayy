using Microsoft.Extensions.Options;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO.Paymob;
using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using Project.Core.Settings;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Security.Cryptography; // مهم عشان HMAC
using System.Text;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobSettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBookingService _eventBookingService;

        public PaymentService(
            HttpClient httpClient,
            IOptions<PaymobSettings> settings,
            IUnitOfWork unitOfWork,
            IEventBookingService eventBookingService)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _eventBookingService = eventBookingService;
        }

        // =========================================================================
        // الجزء الأول: بدء عملية الدفع (Initiation)
        // =========================================================================
        public async Task<string> InitiatePaymentAsync(InitiatePaymentDto dto)
        {
            decimal amountToPay = 0;

            // 1. تحديد نوع الدفع (اشتراك ولا تذكرة؟)
            if (dto.PlanId != Guid.Empty)
            {
                var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(dto.PlanId);
                if (plan == null) throw new Exception("الباقة غير موجودة");

                // 👈 هنا بناخد الـ Price من الباقة مباشرة زي ما إنت عاوز
                amountToPay = plan.Price;
            }
            else if (dto.EventBookingId != Guid.Empty)
            {
                var booking = await _unitOfWork.EventBookings.GetByIdAsync((Guid)dto.EventBookingId);
                if (booking == null) throw new Exception("الحجز غير موجود");
                if (booking.Status != BookingStatus.Pending) throw new Exception("الحجز ليس في حالة انتظار الدفع");

                // هنا بنحسب السعر من الإيفنت
                amountToPay = booking.TicketQuantity * booking.Event.Price;
            }
            else
            {
                throw new Exception("يجب تحديد باقة أو حجز للدفع.");
            }
            // 2. الخطوة الأولى: المصادقة مع Paymob
            var authToken = await GetAuthToken();

            // 3. الخطوة الثانية: تسجيل الأوردر
            // السعر في Paymob بالقروش (نضرب في 100)
            string amountCents = (amountToPay * 100).ToString();
            var paymobOrderId = await RegisterOrder(authToken, amountCents);

            // 4. الخطوة الثالثة: طلب مفتاح الدفع (Payment Key)
            var paymentKey = await GetPaymentKey(authToken, paymobOrderId.ToString(), amountCents);

            // 5. حفظ العملية في الداتابيز (Pending)
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = amountToPay,
                Currency = "EGP",
                PaymentMethod = "Card",
                EventBookingId = dto.EventBookingId,
                Status = "Pending",
                TransactionDate = DateTime.UtcNow,
                PaymobOrderId = paymobOrderId,
                SubscriptionId = null,

                // 🟢 التعديل الجديد: حفظ بيانات البيزنس والباقة
                BusinessId = dto.BusinessId,
                PlanId = dto.PlanId
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return paymentKey;
        }

        // =========================================================================
        // الجزء الثاني: معالجة الرد (Webhook Processing)
        // =========================================================================
        public async Task ProcessWebhookAsync(PaymobWebhookDto dto)
        {
            // 1. التأمين: التحقق من HMAC Signature
            //if (!ValidateHmac(dto))
            //    throw new Exception("Invalid HMAC signature");

            var transaction = dto.Obj;
            long paymobOrderId = transaction.Order.Id;

            // 2. البحث عن العملية في الداتابيز
            var payment = await _unitOfWork.Payments.GetByPaymobOrderIdAsync(paymobOrderId);

            // لو مش موجودة أو حالتها اتغيرت قبل كده، نوقف
            if (payment == null || payment.Status != "Pending") return;

            // 3. تحديث بيانات العملية
            payment.PaymobTransactionId = transaction.Id;
            payment.TransactionDate = DateTime.UtcNow;

            if (transaction.Success)
            {
                payment.Status = "Success";

                // 👈 بنتشك لو الـ PlanId مش فاضي
                if (payment.PlanId != Guid.Empty && payment.PlanId != null)
                {
                    // ده اشتراك بيزنس
                    await ActivateSubscriptionAsync(payment);
                }
                // 👈 بنتشك لو الـ EventBookingId مش فاضي
                else if (payment.EventBookingId != Guid.Empty && payment.EventBookingId != null)
                {
                    var confirmDto = new ConfirmPaymentDto
                    {
                        BookingId = (Guid)payment.EventBookingId, // بنعمل كاستنج سريع للضمان
                        TransactionId = transaction.Id.ToString(),
                        PaymentMethod = PaymentMethod.CreditCard
                    };

                    await _eventBookingService.ConfirmPaymentAsync(Guid.Empty, confirmDto);
                }
            }
            else
            {
                // ❌ حالة الفشل
                payment.Status = "Failed";
            }

            // حفظ التغييرات النهائية
            await _unitOfWork.SaveChangesAsync();
        }

        // =========================================================================
        // دوال مساعدة خاصة (Private Helpers)
        // =========================================================================

        private async Task ActivateSubscriptionAsync(Payment payment)
        {
            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(payment.PlanId);
            if (plan == null) return;

            // إنشاء اشتراك جديد
            var newSubscription = new BusinessSubscription
            {
                Id = Guid.NewGuid(),
                BusinessId = payment.BusinessId,
                PlanId = payment.PlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                IsActive = true,
                AutoRenew = false
            };

            await _unitOfWork.BusinessSubscriptions.AddAsync(newSubscription);

            // ربط الدفع بالاشتراك الجديد
            payment.SubscriptionId = newSubscription.Id;
        }

        private bool ValidateHmac(PaymobWebhookDto dto)
        {
            var data = dto.Obj;

            // ترتيب البيانات ده إجباري من Paymob
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

            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HmacSecret.Trim())))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));
                var calculatedHmac = BitConverter.ToString(hash).Replace("-", "").ToLower();

                return calculatedHmac.Equals(dto.Hmac, StringComparison.OrdinalIgnoreCase);
            }
        }

        private async Task<string> GetAuthToken()
        {
            var request = new PaymobAuthRequest { ApiKey = _settings.ApiKey };
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>();
            return result.Token;
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
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobOrderResponse>();
            return result.Id;
        }

        private async Task<string> GetPaymentKey(string authToken, string orderId, string amountCents)
        {
            var request = new PaymobKeyRequest
            {
                AuthToken = authToken,
                AmountCents = amountCents,
                Expiration = 3600,
                OrderId = orderId,
                BillingData = new PaymobBillingData
                {
                    // بيانات وهمية (يفضل تغييرها ببيانات حقيقية من جدول البيزنس لو متاحة)
                    Country = "EG", // أهم حاجة دي تكون EG مش NA
                    City = "Cairo",
                    FirstName = "Mohamed",
                    LastName = "Test",
                    PhoneNumber = "+201012345678",
                    Email = "test@test.com",
                },
                Currency = "EGP",
                IntegrationId = _settings.IntegrationId
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobKeyResponse>();
            return result.Token;
        }
    }
}