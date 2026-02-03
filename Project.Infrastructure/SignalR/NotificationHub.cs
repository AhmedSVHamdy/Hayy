using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.SignalR
{
    // [Authorize] مهمة جداً عشان مفيش حد غريب يدخل القناة
    // بتخلي الـ Hub يعرف الـ User ID من التوكن (JWT) اللي مبعوت من الموبايل أو الويب
    [Authorize]
    public class NotificationHub : Hub
    {
        // دالة بتشتغل أول ما اليوزر يفتح التطبيق ويعمل اتصال
        public override async Task OnConnectedAsync()
        {
            // ممكن هنا تسجل في الداتابيز إن اليوزر "Online"
            // string userId = Context.UserIdentifier;

            await base.OnConnectedAsync();
        }

        // دالة بتشتغل لما اليوزر يقفل التطبيق أو النت يقطع
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // ممكن هنا تسجل إن اليوزر "Offline"
            await base.OnDisconnectedAsync(exception);
        }
    }
}
