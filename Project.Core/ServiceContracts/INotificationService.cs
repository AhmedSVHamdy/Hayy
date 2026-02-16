using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface INotificationService
    {
        // 1. إنشاء إشعار جديد
        Task<NotificationResponse> CreateNotification(NotificationAddRequest request);

        // 2. جلب إشعارات مستخدم معين
        // (غيرت اسمها لـ GetUserNotifications عشان تبقى أوضح إنها بتاخد UserId)
        Task<List<NotificationResponse>> GetUserNotifications(Guid userId);

        // 3. قراءة إشعار واحد (مسحت الدالة المكررة وخليت دي بس)
        Task MarkAsReadAsync(Guid notificationId, Guid userId);

        // 4. قراءة كل الإشعارات
        Task MarkAllAsReadAsync(Guid userId);

        // 5. عداد الإشعارات غير المقروءة
        Task<int> GetUnreadCountAsync(Guid userId);

        Task<PagedResult<NotificationResponse>> GetUserNotificationsPaged(Guid userId, int pageNumber, int pageSize);
    }
}
