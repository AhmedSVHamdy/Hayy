//using Microsoft.Extensions.Options;
//using Project.Core.Domain.Entities;
//using Project.Core.Domain.RepositoryContracts;
//using Project.Core.DTO;
//using Project.Core.DTOs.Paymob;
//using Project.Core.Enums;
//using Project.Core.Settings;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Project.Core.Services
//{
//    public class EventPaymentService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly PaymobSettings _settings;
//        private readonly IUnitOfWork _unitOfWork;

//        public EventPaymentService(
//            HttpClient httpClient,
//            IOptions<PaymobSettings> settings,
//            IUnitOfWork unitOfWork)
//        {
//            _httpClient = httpClient;
//            _settings = settings.Value;
//            _unitOfWork = unitOfWork;
//        }

//        // 1. دالة بدء الدفع للحجوزات
//        public async Task<string> InitiateEventPaymentAsync(InitiateEventPaymentDto dto)
//        {
//            // 1. جلب الإيفينت والتحقق منه
//            var eventEntity = await _unitOfWork.Events.GetByIdAsync(dto.EventId);
//            if (eventEntity == null)
//                throw new Exception("الحدث غير موجود");

//            decimal amountToPay = eventEntity.TicketPrice; // افترضت اسم حقل السعر

//            // 2. المصادقة مع Paymob
//            var authToken = await GetAuthToken(); // استخدم نفس الدالة المساعدة اللي تحت

//            // 3. تسجيل الأوردر عند Paymob
//            string amountCents = (amountToPay * 100).ToString();
//            var paymobOrderId = await RegisterOrder(authToken, amountCents);

//            // 4. الحصول على مفتاح الدفع (لو الإيفينتات ليها Integration ID مختلف، هتحتاج تبعته هنا)
//            var paymentKey = await GetPaymentKey(authToken, paymobOrderId.ToString(), amountCents);

//            // 5. حفظ العملية في الداتابيز بحالة Pending
//            var payment = new Payment
//            {
//                Id = Guid.NewGuid(),
//                Amount = amountToPay,
//                Currency = "EGP",
//                PaymentMethod = PaymentMethod.CreditCard,
//                Status = PaymentStatus.Pending,
//                TransactionDate = DateTime.UtcNow,
//                PaymobOrderId = paymobOrderId,

//                // هنا بنسجل بيانات الإيفينت واليوزر بدل البزنس
//                UserId = dto.UserId,
//                EventId = dto.EventId,
//                Purpose = PaymentPurpose.EventBooking // لو ضفت الـ Enum
//            };

//            await _unitOfWork.Payments.AddAsync(payment);
//            await _unitOfWork.SaveChangesAsync();

//            return paymentKey;
//        }

//        // 2. دالة معالجة الـ Webhook الخاصة بالحجوزات
//        public async Task ProcessEventWebhookAsync(PaymobWebhookDto dto)
//        {
//            if (!ValidateHmac(dto))
//                throw new Exception("Invalid HMAC signature");

//            var transaction = dto.Obj;
//            long paymobOrderId = transaction.Order.Id;

//            var payment = await _unitOfWork.Payments.GetByPaymobOrderIdAsync(paymobOrderId);

//            if (payment == null || payment.Status != PaymentStatus.Pending)
//                return;

//            payment.PaymobTransactionId = transaction.Id;
//            payment.TransactionDate = DateTime.UtcNow;

//            if (transaction.Success)
//            {
//                payment.Status = PaymentStatus.Completed;

//                // 👇 هنا بتضيف كود تأكيد حجز الإيفينت لليوزر
//                await ConfirmEventBookingAsync(payment);
//            }
//            else
//            {
//                payment.Status = PaymentStatus.Failed;
//            }

//            await _unitOfWork.SaveChangesAsync();
//        }

//        // دالة مساعدة لتأكيد الحجز
//        private async Task ConfirmEventBookingAsync(Payment payment)
//        {
//            // هنا بتكتب اللوجيك بتاعك لإضافة التذكرة لليوزر
//            // مثلا:
//            // var ticket = new EventTicket { UserId = payment.UserId, EventId = payment.EventId ... };
//            // await _unitOfWork.EventTickets.AddAsync(ticket);
//        }
//    }
//}
