using Microsoft.AspNetCore.SignalR;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.SignalR
{
    public class SignalRNotifier : INotifier
    {
        // بنكلم السنترال (Hub) عشان نستخدمه في الإرسال
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(Guid userId, NotificationResponse notification)
        {
            // الـ UserId هنا بيتحول لـ String لأن SignalR بيتعامل مع الـ IDs كنصوص
            // "ReceiveNotification" ده اسم الدالة اللي الفرونت (Flutter/React) لازم يسمع عليها
            if (userId != Guid.Empty)
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }
        }
        public async Task SendNotificationToGroup(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", message);
        }
        public async Task SendNotificationToUser(string userId, string message)
        {
            // SignalR ذكي، هيدور على اليوزر اللي الـ Claim ID بتاعه بيساوي userId
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task SendNotificationToUserWaitlist(string userId, string message)
        {
            // SignalR هيدور على اليوزر اللي عامل Login والـ ID بتاعه بيطابق الـ userId
            await _hubContext.Clients.User(userId).SendAsync("WaitlistNotification", new
            {
                Message = message,
                Status = "Pending",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// إرسال إشعار فوري (Real-time) للـ followers عن طريق SignalR
        /// </summary>
        public async Task NotifyFollowersRealtimeAsync(
            Guid placeId,
            string title,
            string message,
            string referenceId,
            string referenceType,
            string notificationType)
        {
            // إنشاء الإشعار
            var notification = new
            {
                Title = title,
                Message = message,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                NotificationType = notificationType,
                Timestamp = DateTime.UtcNow
            };

            // إرسال للـ followers الموصلين حالياً عبر جروب المكان
            string groupName = $"Place_{placeId}_Followers";
            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
