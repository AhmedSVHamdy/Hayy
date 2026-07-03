using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface INotifier
    {
        // الدالة دي وظيفتها: "يا سيستم، وصل الرسالة دي لليوزر ده"
        Task SendToUserAsync(Guid userId, NotificationResponse notification);
        Task SendNotificationToGroup(string groupName, string message);
        Task SendNotificationToUser(string groupName, string message);
        Task SendNotificationToUserWaitlist(string userId, string message);

        // 🆕 إرسال إشعار فوري (Real-time) للـ followers
        Task NotifyFollowersRealtimeAsync(
            Guid placeId,
            string title,
            string message,
            string referenceId,
            string referenceType,
            string notificationType);
    }
}
