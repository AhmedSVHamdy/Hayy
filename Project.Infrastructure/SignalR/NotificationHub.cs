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
            string userId = Context.UserIdentifier;

            await base.OnConnectedAsync();
        }

        // دالة بتشتغل لما اليوزر يقفل التطبيق أو النت يقطع
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // ممكن هنا تسجل إن اليوزر "Offline"
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// دالة يستدعيها المستخدم عند الـ follow على مكان معين
        /// عشان ينضم لـ جروب الإشعارات بتاع المكان
        /// </summary>
        public async Task JoinPlaceFollowersGroup(string placeId)
        {
            if (string.IsNullOrEmpty(placeId))
                throw new ArgumentException("Place ID cannot be empty");

            // إضافة الـ connection بتاع المستخدم للجروب
            string groupName = $"Place_{placeId}_Followers";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // ✅ رد تأكيد للمستخدم
            await Clients.Caller.SendAsync("JoinedGroup", new
            {
                Message = $"تم الانضمام لجروب متابعة المكان بنجاح!",
                GroupName = groupName,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// دالة يستدعيها المستخدم عند إلغاء الـ follow من مكان معين
        /// </summary>
        public async Task LeaveePlaceFollowersGroup(string placeId)
        {
            if (string.IsNullOrEmpty(placeId))
                throw new ArgumentException("Place ID cannot be empty");

            // حذف الـ connection من الجروب
            string groupName = $"Place_{placeId}_Followers";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            // ✅ رد تأكيد للمستخدم
            await Clients.Caller.SendAsync("LeftGroup", new
            {
                Message = $"تم إلغاء الانضمام من جروب المكان بنجاح!",
                GroupName = groupName,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
