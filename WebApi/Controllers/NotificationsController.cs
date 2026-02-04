using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.Security.Claims;

namespace WebApi.Controllers
{
    /// <summary>
    /// Represents an API controller for managing user notifications, including creating new notifications, retrieving
    /// notifications for the current user, and marking notifications as read.
    /// </summary>
    /// <remarks>All endpoints in this controller require the caller to be authenticated. The controller is
    /// intended for use by authorized users only, and some actions (such as creating notifications) are typically
    /// performed by administrators or system processes. The current user's identity is determined from the
    /// authentication token rather than from request parameters, enhancing security by preventing spoofing of user
    /// IDs.</remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔐 مهم جداً: محدش يستخدم الكنترولر ده غير لما يكون مسجل دخول
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // 1. 📤 إرسال إشعار جديد (للأدمن أو السيستم)
        [HttpPost]
        //[Authorize(Roles = "Admin")] يفضل تحط عليها قيد إن "الأدمن" بس هو اللي يقدر يندهها، عشان مش أي يوزر يبعت إشعارات ليوزر تاني بمزاجه.
        public async Task<IActionResult> Create(NotificationAddRequest request)
        {
            var result = await _notificationService.CreateNotification(request);
            return Ok(result);
        }

        // 2. 📜 جلب كل إشعارات المستخدم الحالي
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _notificationService.GetUserNotifications(userId);
            return Ok(result);
        }

        // 3. 🔴 جلب عدد الإشعارات غير المقروءة (عشان العداد في الـ Navbar)
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count }); // بيرجع JSON زي { "count": 5 }
        }

        // 4. ✅ تعليم إشعار واحد كمقروء (لما يضغط عليه)
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { Message = "Notification marked as read" });
        }

        // 5. ✅✅ تعليم الكل كمقروء (زرار Mark All as Read)
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { Message = "All notifications marked as read" });
        }

        // 🛠️ دالة مساعدة لجلب الـ ID من التوكن (عشان منكررش الكود)
        private Guid GetCurrentUserId()
        {
            // حاول يجيب الـ ID من الـ ClaimTypes القياسي أو الاسم اللي انت بتستخدمه "uid"
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;

            if (Guid.TryParse(idClaim, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}
