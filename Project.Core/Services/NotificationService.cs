using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.Entities.NotificationPayload;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Project.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IMapper _mapper;
        private readonly INotifier _notifier; // 1. ضفنا الكوبري بتاع SignalR

        public NotificationService(
            INotificationRepository repo,
            IMapper mapper,
            INotifier notifier) // 2. حقناه في الـ Constructor
        {
            _repo = repo;
            _mapper = mapper;
            _notifier = notifier;
        }

        public async Task<NotificationResponse> CreateNotification(NotificationAddRequest request)
        {
            Notification notification;
            NotificationResponse response;

            // 3. هل فيه GroupKey؟ وهل فيه إشعار قديم لسه مقروش؟
            Notification? existingNotification = null;
            if (!string.IsNullOrEmpty(request.GroupKey))
            {
                existingNotification = await _repo.GetUnreadByGroupKeyAsync(request.UserId, request.GroupKey);
            }

            if (existingNotification != null)
            {
                // =================================================
                //  SCENARIO A: UPDATE (تحديث إشعار موجود) 🔄
                // =================================================

                // أ) فك الـ Payload القديم عشان نعرف العدد
                var dataHelper = existingNotification.Payload != null
                    ? JsonSerializer.Deserialize<NotificationData>(existingNotification.Payload)
                    : new NotificationData();

                // ب) حساب البيانات الجديدة
                int newCount = (dataHelper?.ItemCount ?? 1) + 1;
                string newActorName = request.Data?.UserName ?? "شخص ما"; // الاسم الجديد

                // ج) تحديث الكلاس المساعد (NotificationData)
                if (dataHelper != null)
                {
                    dataHelper.ItemCount = newCount;
                    dataHelper.UserName = newActorName; // تحديث الاسم لآخر واحد عمل أكشن
                    dataHelper.UserImage = request.Data?.UserImage ?? dataHelper.UserImage; // تحديث الصورة
                }

                // د) تحديث بيانات الإشعار نفسه (Entity)
                existingNotification.Message = $"{newActorName} و {newCount - 1} آخرون تفاعلوا مع منشورك";
                existingNotification.Title = "تفاعل جديد 🔥";
                existingNotification.CreatedAt = DateTime.UtcNow; // تجديد الوقت عشان يطلع فوق

                // هـ) إعادة تغليف الـ Payload وحفظه
                existingNotification.Payload = JsonSerializer.Serialize(dataHelper);

                // و) حفظ التعديل في الداتابيز
                await _repo.UpdateAsync(existingNotification);

                // ز) نعتمد المتغير ده عشان نرجعه
                notification = existingNotification;
            }
            else
            {
                // =================================================
                //  SCENARIO B: CREATE (إنشاء إشعار جديد) 🆕
                // =================================================

                notification = _mapper.Map<Notification>(request);

                // تظبيط العدد المبدئي بـ 1
                if (request.Data != null) request.Data.ItemCount = 1;

                // المابر بيحول الـ Data لـ JSON String أوتوماتيك هنا حسب إعدادات الـ Profile
                // بس لو المابر مش مظبوط، ممكن نأكد عليه يدوياً:
                // notification.Payload = JsonSerializer.Serialize(request.Data);

                await _repo.AddAsync(notification);
            }

            // 4. تحويل الـ Entity لـ Response
            response = _mapper.Map<NotificationResponse>(notification);

            // 5. إرسال Real-Time Notification (SignalR) 📡
            // بنحطها في Try-Catch عشان لو السيرفر فيه مشكلة في الاتصال، العملية الأصلية متقفش
            try
            {
                await _notifier.SendToUserAsync(request.UserId, response);
            }
            catch
            {
                // ممكن تعمل Log هنا (Console.WriteLine("SignalR Failed"))
            }

            return response;
        }

        public async Task<List<NotificationResponse>> GetMyNotifications(Guid userId)
        {
            var notifications = await _repo.GetUserNotificationsAsync(userId);
            return _mapper.Map<List<NotificationResponse>>(notifications);
        }

        public async Task<bool> MarkNotificationAsRead(Guid notificationId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null) return false;

            await _repo.MarkAsReadAsync(notification);
            return true;
        }
    }
}
