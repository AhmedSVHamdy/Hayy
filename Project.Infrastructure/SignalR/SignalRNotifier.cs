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
    }
}
