using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.Entities.NotificationPayload;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Helpers; // تأكد من الـ Namespace ده
using Project.Core.ServiceContracts;
using System.Text.Json;

namespace Project.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IMapper _mapper;
        private readonly INotifier _notifier; // SignalR
        private readonly IUnitOfWork _unitOfWork; 


        public NotificationService(
            INotificationRepository repo,
            IMapper mapper,
            INotifier notifier,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _mapper = mapper;
            _notifier = notifier;
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // 1. إنشاء إشعار (مع Grouping Logic)
        // =========================================================
        public async Task<NotificationResponse> CreateNotification(NotificationAddRequest request)
        {
            var userSettings = await _unitOfWork.GetRepository<UserSettings>()
        .GetAsync(s => s.UserId == request.UserId);

            // إذا كان المستخدم قد عطل الإشعارات تماماً، يمكننا التوقف هنا أو إكمال التسجيل فقط بدون إرسال
            bool isNotificationEnabled = userSettings?.NotificationsEnabled ?? true;
            Notification notification;
            Notification? existingNotification = null;

            // 1. التحقق من وجود GroupKey (تجميع الإشعارات)
            if (!string.IsNullOrEmpty(request.GroupKey))
            {
                // الدالة دي موجودة في الريبو الجديد ✅
                existingNotification = await _repo.GetUnreadByGroupKeyAsync(request.UserId, request.GroupKey);
            }

            if (existingNotification != null)
            {
                // --- A) حالة التحديث (Update Existing) ---
                var dataHelper = !string.IsNullOrEmpty(existingNotification.Payload)
                    ? JsonSerializer.Deserialize<NotificationData>(existingNotification.Payload)
                    : new NotificationData();

                // تحديث البيانات
                int newCount = (dataHelper?.ItemCount ?? 1) + 1;
                string newActorName = request.Data?.UserName ?? "مستخدم";

                if (dataHelper != null)
                {
                    dataHelper.ItemCount = newCount;
                    dataHelper.UserName = newActorName;
                    if (!string.IsNullOrEmpty(request.Data?.UserImage))
                        dataHelper.UserImage = request.Data.UserImage;
                }

                // تحديث الحقول
                existingNotification.Title = request.Title;
                existingNotification.Message = $"{newActorName} و {newCount - 1} آخرون تفاعلوا معك";
                existingNotification.CreatedAt = DateTime.UtcNow; // رفعه للأحدث
                existingNotification.IsRead = false; // تنبيه جديد
                existingNotification.Payload = JsonSerializer.Serialize(dataHelper);

                await _repo.UpdateAsync(existingNotification);
                notification = existingNotification;
            }
            else
            {
                // --- B) حالة الإنشاء الجديد (Create New) ---
                if (request.Data != null) request.Data.ItemCount = 1;

                notification = _mapper.Map<Notification>(request);
                notification.CreatedAt = DateTime.UtcNow;

                // إضافة للداتابيز
                await _repo.AddAsync(notification);
            }

            // 2. التحويل والرد (Mapping)
            var response = _mapper.Map<NotificationResponse>(notification);

            // 3. إرسال Real-Time (SignalR) 📡
            if (isNotificationEnabled)
            {
                try
                {
                    await _notifier.SendToUserAsync(request.UserId, response);
                }
                catch (Exception) { /* تجاهل أخطاء السوكيت */ }
            }

            return response;
        }

        // =========================================================
        // 2. جلب إشعارات المستخدم (Paged)
        // =========================================================
        public async Task<PagedResult<NotificationResponse>> GetUserNotificationsPaged(Guid userId, int pageNumber, int pageSize)
        {
            // تنظيف المدخلات
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            // جلب البيانات (بالاسم الجديد في الريبو) ✅
            var notifications = await _repo.GetByUserIdPagedAsync(userId, pageNumber, pageSize);

            // جلب العدد الكلي (بالاسم الجديد في الريبو) ✅
            var totalCount = await _repo.GetTotalCountAsync(userId);

            // التحويل
            var dtos = _mapper.Map<List<NotificationResponse>>(notifications);

            return new PagedResult<NotificationResponse>(dtos, totalCount, pageNumber, pageSize);
        }

        // =========================================================
        // 3. قراءة إشعار واحد (Mark As Read)
        // =========================================================
        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            int rowsAffected = await _repo.MarkAsReadAsync(notificationId, userId);

            if (rowsAffected == 0)
            {
                // 🚨 هنا المشكلة!
                // لو دخلنا هنا، يبقى الداتابيز بتقول: "أنا مش لاقيه إشعار بالـ ID ده لليوزر ده"
                throw new KeyNotFoundException($"فشل التعديل! الإشعار {notificationId} غير موجود أو لا يخص المستخدم {userId}");
            }
        }

        // =========================================================
        // 4. قراءة الكل (Mark All As Read) - Optimized 🚀
        // =========================================================
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            // استخدام الدالة السريعة المباشرة في الريبو ✅
            // بدلاً من اللوب القديم (UpdateRangeAsync)
            await _repo.MarkAllAsReadAsync(userId);
        }

        // =========================================================
        // 5. العداد (Unread Count)
        // =========================================================
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            // بالاسم الجديد المتطابق ✅
            return await _repo.GetUnreadCountAsync(userId);
        }
    }
}