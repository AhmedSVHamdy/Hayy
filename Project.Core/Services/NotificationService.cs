using AutoMapper;
using FluentValidation; 
using Project.Core.Domain.Entities;
using Project.Core.Domain.Entities.NotificationPayload;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.Text.Json;

namespace Project.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IMapper _mapper;
        private readonly INotifier _notifier; // SignalR
        private readonly IValidator<NotificationAddRequest> _validator; // 👈 الحقن هنا

        public NotificationService(
            INotificationRepository repo,
            IMapper mapper,
            INotifier notifier,
            IValidator<NotificationAddRequest> validator) // بنستلم الفاليداتور
        {
            _repo = repo;
            _mapper = mapper;
            _notifier = notifier;
            _validator = validator;
        }

        // =========================================================
        // 1. إنشاء إشعار (مع Validation + Grouping)
        // =========================================================
        public async Task<NotificationResponse> CreateNotification(NotificationAddRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                // بنجمع الأخطاء ونرميها عشان الميدلوير يرجعها 400 Bad Request
                var errorMsg = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errorMsg);
            }

            Notification notification;
            Notification? existingNotification = null;

            if (!string.IsNullOrEmpty(request.GroupKey))
            {
                existingNotification = await _repo.GetUnreadByGroupKeyAsync(request.UserId, request.GroupKey);
            }

            if (existingNotification != null)
            {
                // --- A) حالة التحديث (Update Existing) ---

                // فك التشفير (Deserialize) عشان نحدث العداد
                var dataHelper = !string.IsNullOrEmpty(existingNotification.Payload)
                    ? JsonSerializer.Deserialize<NotificationData>(existingNotification.Payload)
                    : new NotificationData();

                // تحديث العداد والأسماء
                int newCount = (dataHelper?.ItemCount ?? 1) + 1;
                string newActorName = request.Data?.UserName ?? "مستخدم";

                if (dataHelper != null)
                {
                    dataHelper.ItemCount = newCount;
                    dataHelper.UserName = newActorName;
                    // لو جاي صورة جديدة خدها، لو لأ خلي القديمة
                    if (!string.IsNullOrEmpty(request.Data?.UserImage))
                        dataHelper.UserImage = request.Data.UserImage;
                }

                // تحديث نصوص الإشعار
                existingNotification.Title = request.Title; // تحديث العنوان بآخر حدث
                existingNotification.Message = $"{newActorName} و {newCount - 1} آخرون تفاعلوا معك";
                existingNotification.CreatedAt = DateTime.UtcNow; // رفعه للأحدث
                existingNotification.IsRead = false; // نخليه غير مقروء تاني عشان ينبه اليوزر
                existingNotification.Payload = JsonSerializer.Serialize(dataHelper); // حفظ الداتا الجديدة

                await _repo.UpdateAsync(existingNotification);
                notification = existingNotification;
            }
            else
            {
                // --- B) حالة الإنشاء الجديد (Create New) ---

                // نضبط العداد بـ 1 قبل المابينج
                if (request.Data != null) request.Data.ItemCount = 1;

                notification = _mapper.Map<Notification>(request);
                // المابر هنا هيقوم بالواجب ويحول الـ Data لـ JSON String أوتوماتيك (حسب الـ Profile اللي عملناه)

                notification.CreatedAt = DateTime.UtcNow;
                await _repo.AddAsync(notification);
            }

            // 🛑 3. التحويل والرد (Response)
            var response = _mapper.Map<NotificationResponse>(notification);

            // 🛑 4. إرسال Real-Time (SignalR) 📡
            try
            {
                await _notifier.SendToUserAsync(request.UserId, response);
            }
            catch (Exception)
            {
                // بنعمل Catch عشان لو السوكيت واقع، الداتابيز متتأثرش وتكمل عادي
            }

            return response;
        }

        // =========================================================
        // 2. جلب إشعارات المستخدم
        // =========================================================
        public async Task<List<NotificationResponse>> GetUserNotifications(Guid userId)
        {
            var notifications = await _repo.GetByUserIdAsync(userId);
            return _mapper.Map<List<NotificationResponse>>(notifications);
        }

        // =========================================================
        // 3. قراءة إشعار واحد
        // =========================================================
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _repo.UpdateAsync(notification);
            }
        }

        // =========================================================
        // 4. قراءة كل الإشعارات
        // =========================================================
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _repo.GetUnreadByUserIdAsync(userId);

            if (unreadNotifications != null && unreadNotifications.Any())
            {
                foreach (var note in unreadNotifications)
                {
                    note.IsRead = true;
                }
                await _repo.UpdateRangeAsync(unreadNotifications);
            }
        }

        // =========================================================
        // 5. عداد الإشعارات غير المقروءة
        // =========================================================
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _repo.CountUnreadAsync(userId);
        }
    }
}