using Microsoft.Extensions.Options;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO.Paymob;
using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using Project.Core.Settings;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Project.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobSettings _settings;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            HttpClient httpClient,
            IOptions<PaymobSettings> settings,
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
        }

        // =========================================================================
        // الجزء الأول: بدء عملية الدفع
        // =========================================================================
        public async Task<string> InitiatePaymentAsync(InitiatePaymentDto dto)
        {
            // 1. جلب الباقة والتحقق منها
            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(dto.PlanId);
            if (plan == null)
                throw new Exception("الباقة غير موجودة");

            decimal amountToPay = plan.Price;

            // 2. المصادقة مع Paymob
            var authToken = await GetAuthToken();

            // 3. تسجيل الأوردر عند Paymob (السعر بالقروش = نضرب × 100)
            string amountCents = (amountToPay * 100).ToString();
            var paymobOrderId = await RegisterOrder(authToken, amountCents);

            // 4. الحصول على مفتاح الدفع
            var paymentKey = await GetPaymentKey(authToken, paymobOrderId.ToString(), amountCents);

            // 5. حفظ العملية في الداتابيز بحالة Pending
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = amountToPay,
                Currency = "EGP",
                PaymentMethod = PaymentMethod.CreditCard, // ✅ Enum
                Status = PaymentStatus.Pending,           // ✅ Enum
                TransactionDate = DateTime.UtcNow,
                PaymobOrderId = paymobOrderId,
                BusinessId = dto.BusinessId,
                PlanId = dto.PlanId,
                SubscriptionId = null
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return paymentKey;
        }

        // =========================================================================
        // الجزء الثاني: معالجة رد Paymob (Webhook)
        // =========================================================================
        public async Task ProcessWebhookAsync(PaymobWebhookDto dto)
        {
            // 1. التحقق من الـ HMAC لضمان إن الطلب قادم من Paymob فعلاً
            if (!ValidateHmac(dto))
                throw new Exception("Invalid HMAC signature");

            var transaction = dto.Obj;
            long paymobOrderId = transaction.Order.Id;

            // 2. البحث عن العملية في الداتابيز
            var payment = await _unitOfWork.Payments.GetByPaymobOrderIdAsync(paymobOrderId);

            // لو مش موجودة أو اتعالجت قبل كده → نتجاهل
            if (payment == null || payment.Status != PaymentStatus.Pending) // ✅ Enum
                return;

            // 3. تحديث بيانات العملية
            payment.PaymobTransactionId = transaction.Id;
            payment.TransactionDate = DateTime.UtcNow;

            if (transaction.Success)
            {
                payment.Status = PaymentStatus.Completed; // ✅ Enum
                await ActivateSubscriptionAsync(payment);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;    // ✅ Enum
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // =========================================================================
        // دوال مساعدة خاصة
        // =========================================================================

        private async Task ActivateSubscriptionAsync(Payment payment)
        {
            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(payment.PlanId);
            if (plan == null) return;

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

            // ترتيب الحقول ده إجباري من Paymob - لا تغيره
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

        private async Task<string> GetAuthToken()
        {
            var request = new PaymobAuthRequest { ApiKey = _settings.ApiKey };
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", request);
            response.EnsureSuccessStatusCode();
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
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobOrderResponse>();
            return result!.Id;
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
                    Country = "EG",
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
            return result!.Token;
        }
    }
}