using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface INotificationService
    {
        // 1. إنشاء إشعار (بيرجع الـ Response عشان الفرونت يحدث اللسته)
        Task<NotificationResponse> CreateNotification(NotificationAddRequest request);

        // 2. جلب القائمة (Pagination)
        Task<PagedResult<NotificationResponse>> GetUserNotificationsPaged(Guid userId, int pageNumber, int pageSize);

        // 3. قراءة إشعار واحد
        Task MarkAsReadAsync(Guid notificationId, Guid userId);

        // 4. قراءة الكل (تحديث سريع)
        Task MarkAllAsReadAsync(Guid userId);

        // 5. العداد
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
