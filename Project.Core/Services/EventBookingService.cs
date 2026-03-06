using AutoMapper;
using FluentValidation;
using Hangfire;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.DTO.Paymob;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventBooking;

namespace Project.Core.Services
{
    public class EventBookingService : IEventBookingService
    {
        private readonly IEventBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBookingDto> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public EventBookingService(
            IEventBookingRepository bookingRepository,
            IEventRepository eventRepository,
            IMapper mapper,
            IValidator<CreateBookingDto> validator,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(Guid userId, CreateBookingDto dto)
        {
            // 1. Validation
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ArgumentException(validationResult.Errors.First().ErrorMessage);

            // 2. جلب الحدث (باستخدام UoW)
            // لاحظ إننا بنستخدم الدالة الذكية بتاعتك GetRepository
            var @event = await _unitOfWork.GetRepository<Event>().GetByIdAsync(dto.EventId);
            if (@event == null || @event.Status != EventStatus.Active)
                throw new ArgumentException("هذا الحدث غير متاح حالياً.");

            // 3. حساب التذاكر المؤكدة والمعلقة (باستخدام UoW)
            int currentValidBookings = await _unitOfWork.EventBookings.GetValidTicketsCountAsync(dto.EventId);

            // 4. تجهيز أوبجيكت الحجز
            var booking = new EventBooking
            {
                UserId = userId,
                EventId = dto.EventId,
                TicketQuantity = dto.TicketQuantity,
                IsPaid = false,
                PaymentMethod = PaymentMethod.CreditCard
            };

            // 5. اللوجيك الحاسم: هل فيه كراسي فاضية؟
            if (currentValidBookings + dto.TicketQuantity <= @event.Capacity)
            {
                // فيه مكان!
                booking.Status = BookingStatus.Pending;
                booking.PaymentDeadline = DateTime.UtcNow.AddMinutes(15); // خليها 15 دقيقة وقت التست عشان تلحق تجرب
            }
            else if (@event.IsWaitlistEnabled)
            {
                // حساب الويت ليست (باستخدام UoW)
                int currentWaitlistCount = await _unitOfWork.EventBookings.GetWaitlistTicketsCountAsync(dto.EventId);

                if (currentWaitlistCount + dto.TicketQuantity <= @event.WaitlistLimit)
                {
                    booking.Status = BookingStatus.Waitlisted;
                    booking.WaitlistPosition = await _unitOfWork.EventBookings.GetMaxWaitlistPositionAsync(@event.Id) + 1;
                }
                else
                {
                    throw new ArgumentException("نفدت جميع التذاكر واكتملت قائمة الانتظار.");
                }
            }
            else
            {
                throw new ArgumentException("عفواً، نفدت جميع التذاكر.");
            }

            // 6. حفظ في الداتابيز (باستخدام UoW لضمان إنهم نفس الـ Context)
            await _unitOfWork.EventBookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync(); // كده هيتحفظ بنسبة مليار في المية

            // 7. تشغيل العسكري
            if (booking.Status == BookingStatus.Pending)
            {
                BackgroundJob.Schedule<IEventBookingService>(
                    service => service.CancelUnpaidBookingAsync(booking.Id),
                    TimeSpan.FromMinutes(15) // غير دي كمان لـ 15 مؤقتاً
                );
            }

            return _mapper.Map<BookingResponseDto>(booking);
        }
        public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            return _mapper.Map<IEnumerable<BookingResponseDto>>(bookings);
        }
        public async Task<BookingResponseDto> ConfirmPaymentAsync(Guid userId, ConfirmPaymentDto dto)
        {
            // 1. نجيب الحجز من الداتابيز
            var booking = await _unitOfWork.EventBookings.GetByIdAsync(dto.BookingId);

            if (booking == null)
                throw new ArgumentException("الحجز غير موجود.");

            // خلينا الشرط ده ذكي: 
            // لو الـ Webhook هو اللي بيكلمنا (userId = Guid.Empty) هيعدي عادي
            // لو اليوزر هو اللي بيكلمنا من الموبايل، هنتأكد إن ده الحجز بتاعه
            if (userId != Guid.Empty && booking.UserId != userId)
                throw new ArgumentException("غير مصرح لك بتعديل هذا الحجز.");

            // 2. نتأكد إن الحجز لسه معلق وماتلغاش (عشان العسكري ميكونش لغاه)
            if (booking.Status != BookingStatus.Pending)
                throw new ArgumentException($"لا يمكن تأكيد هذا الحجز لأن حالته الحالية: {booking.Status}");

            // 🔥 تأمين للمطب: نجيب بيانات الإيفنت عشان محتاجين السعر (Price) والسعة (Capacity)
            var @event = await _unitOfWork.GetRepository<Event>().GetByIdAsync(booking.EventId);
            if (@event == null)
                throw new ArgumentException("الحدث المرتبط بهذا الحجز غير موجود.");

            // 3. تحديث بيانات الحجز لـ "مؤكد" وتوثيق الدفع
            booking.Status = BookingStatus.Confirmed;
            booking.IsPaid = true;
            booking.TransactionId = dto.TransactionId;

            // 👇 السطر بتاعك شغال تمام لأننا ضمنا إن الإيفنت موجود
            booking.PaidAmount = booking.TicketQuantity * @event.Price;

            booking.PaymentDate = DateTime.UtcNow;
            booking.PaymentDeadline = null; // بنوقف عداد الـ 15 دقيقة لأنه خلاص دفع


            // توليد الـ QR Code
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                string textToEncode = booking.BookingCode ?? booking.Id.ToString();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(textToEncode, QRCodeGenerator.ECCLevel.Q);

                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    booking.QrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
                }
            }

            // 4. حفظ تعديلات الحجز الحالي
            await _unitOfWork.SaveChangesAsync();

            // =================================================================
            // 🔥 اللوجيك الجديد: هل الإيفنت اكتمل (Sold Out)؟
            // =================================================================

            // أ. نجيب كل التذاكر اللي اتأكدت للإيفنت ده (يفضل تعملها كدالة في الـ Repo لوحدها للأداء)
            // بس مؤقتاً ممكن تقرأها من الداتابيز كده:
            // (لو الـ @event.EventBookings جاية بـ Include ممكن تستخدمها، لو لأ استخدم الـ Repository)
            int totalConfirmedTickets = await _unitOfWork.EventBookings.GetConfirmedTicketsCountAsync(booking.EventId);

            // ب. هل التذاكر المؤكدة قفلت الإيفنت؟
            if (totalConfirmedTickets >= @event.Capacity)
            {
                // نجيب كل الناس الغلابة اللي لسه في الويت ليست (بالإيميلات بتاعتهم)
                // 💡 الدالة دي إحنا عملناها الخطوة اللي فاتت في الـ Repository
                var waitlistedBookings = await _unitOfWork.EventBookings.GetWaitlistedBookingsWithUsersAsync(booking.EventId);

                foreach (var wlBooking in waitlistedBookings)
                {
                    // 1. نبعت إيميل الاعتذار
                    if (wlBooking.User != null && !string.IsNullOrEmpty(wlBooking.User.Email))
                    {
                        string subject = "عذراً، نفدت جميع التذاكر 😔";
                        string body = $"مرحباً، نأسف لإبلاغك بأنه قد تم حجز جميع تذاكر حدث '{@event.Title}' بالكامل. نتمنى رؤيتك في فعالياتنا القادمة!";

                        await _emailService.SendEmailAsync(wlBooking.User.Email, subject, body);
                    }

                    // 2. نلغي حجزهم من الويت ليست عشان الطابور يتقفل
                    wlBooking.Status = BookingStatus.Cancelled;
                    wlBooking.WaitlistPosition = null;
                }

                // 3. نحفظ الإلغاءات في الداتابيز
                if (waitlistedBookings.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            // =================================================================

            // 5. تحضير الـ Response وتحويل رقم الحجز لـ QR Code
            var responseDto = _mapper.Map<BookingResponseDto>(booking);

            return responseDto;
        }
        public async Task<VerifyTicketResultDto> VerifyTicketAsync(Guid businessUserId, Guid bookingId)
        {
            // 1. نجيب الحجز
            var booking = await _unitOfWork.EventBookings.GetByIdAsync(bookingId);

            if (booking == null)
                return new VerifyTicketResultDto { IsValid = false, Message = "❌ تذكرة مزيفة أو غير موجودة بالأساس!" };

            // 2. نتأكد من حالة التذكرة
            if (booking.Status == BookingStatus.Cancelled)
                return new VerifyTicketResultDto { IsValid = false, Message = "❌ هذا الحجز ملغي ولن يسمح بالدخول!" };

            // بنتشيك لو التذكرة اتعلم عليها مستخدمة أو عدد اللي دخلوا بيساوي عدد التذاكر المحجوزة
            if (booking.Status == BookingStatus.Used || booking.CheckedInCount >= booking.TicketQuantity)
                return new VerifyTicketResultDto { IsValid = false, Message = "⚠️ احترس! هذه التذكرة تم استخدامها بالكامل من قبل!" };

            if (booking.Status == BookingStatus.Confirmed)
            {
                // 3. التذكرة سليمة -> ندخلهم ونحدث العداد والحالة

                // بنخلي عدد اللي دخلوا يساوي عدد التذاكر (يعني لو حاجز 10، الـ 10 دخلوا)
                booking.CheckedInCount = booking.TicketQuantity;

                // بنغير الحالة لـ مستخدمة عشان الـ QR Code يتحرق ومحدش يدخل بيه تاني
                booking.Status = BookingStatus.Used;

                // 💡 مسحنا سطر الـ Update خالص لأن الـ SaveChangesAsync هتكفي وتوفي
                await _unitOfWork.SaveChangesAsync();

                return new VerifyTicketResultDto
                {
                    IsValid = true,
                    Message = $"✅ تذكرة صالحة. تم دخول {booking.TicketQuantity} فرد/أفراد بنجاح!",
                    EventTitle = booking.Event != null ? booking.Event.Title : "الإيفنت",
                    TicketQuantity = booking.TicketQuantity
                };
            }

            return new VerifyTicketResultDto { IsValid = false, Message = "❌ حالة التذكرة غير صالحة للدخول." };
        }
        public async Task CancelUnpaidBookingAsync(Guid bookingId)
        {
            var booking = await _unitOfWork.EventBookings.GetByIdAsync(bookingId);

            if (booking != null && booking.Status == BookingStatus.Pending && !booking.IsPaid)
            {
                // 1. إلغاء الحجز الحالي وتصفير عداده
                booking.Status = BookingStatus.Cancelled;
                booking.PaymentDeadline = null;

                // 2. 🔥 ننده الدالة الجديدة عشان نجيب أول واحد في الويت ليست
                var nextInWaitlist = await _unitOfWork.EventBookings.GetNextInWaitlistAsync(booking.EventId);

                if (nextInWaitlist != null)
                {
                    // نرقيه ونخليه Pending ونديله 15 دقيقة للدفع
                    nextInWaitlist.Status = BookingStatus.Pending;
                    nextInWaitlist.WaitlistPosition = null;
                    nextInWaitlist.PaymentDeadline = DateTime.UtcNow.AddMinutes(2);

                    // نقول لـ Hangfire يصحى لليوزر ده كمان 15 دقيقة
                    BackgroundJob.Schedule<IEventBookingService>(
                        service => service.CancelUnpaidBookingAsync(nextInWaitlist.Id),
                        TimeSpan.FromMinutes(2)
                    );

                    // ==========================================
                    // 🔔 إرسال الإشعار لليوزر اللي دوره جه!
                    // ==========================================
                    // (بفترض إنك عندك Navigation Property اسمها User جوه EventBooking)
                    if (nextInWaitlist.User != null && !string.IsNullOrEmpty(nextInWaitlist.User.Email))
                    {
                        string subject = "دورك جه! تذكرتك متاحة الآن 🎟️";
                        string eventName = nextInWaitlist.Event?.Title ?? "الحدث";
                        string body = $"أهلاً، لقد حان دورك في قائمة الانتظار لحدث '{eventName}'. أمامك 15 دقيقة فقط لإتمام الدفع وإلا سيتم تمرير التذكرة للشخص التالي.";

                        // استخدمنا الـ Fire-and-forget أو await عادي
                        await _emailService.SendEmailAsync(nextInWaitlist.User.Email, subject, body);
                    }
                }

                // 3. حفظ في الداتابيز
                await _unitOfWork.SaveChangesAsync();
            }

        }
        public async Task<BookingResponseDto?> GetUserBookingForEventAsync(Guid userId, Guid eventId)
        {
            // 1. بنكلم الـ Repository بالدالة الجديدة
            var booking = await _unitOfWork.EventBookings.GetBookingByUserAndEventAsync(userId, eventId);

            // 2. لو مفيش حجز هيرجع null
            if (booking == null) return null;

            // 3. لو لقينا الحجز، نحوله لـ DTO ونرجعه
            return _mapper.Map<BookingResponseDto>(booking);
        }

    }
}
