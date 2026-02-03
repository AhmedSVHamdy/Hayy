using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface INotificationService
    {
        // دالة إنشاء إشعار (دي اللي هنستخدمها لما حد يعمل ريفيو أو أوردر)
        Task<NotificationResponse> CreateNotification(NotificationAddRequest request);

        // دالة عرض إشعارات اليوزر
        Task<List<NotificationResponse>> GetMyNotifications(Guid userId);

        // دالة "قراءة" الإشعار
        Task<bool> MarkNotificationAsRead(Guid notificationId);
    }
}
