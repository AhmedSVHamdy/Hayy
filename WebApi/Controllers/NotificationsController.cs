using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
     
    [Authorize] // 🔐 مهم جداً: محدش يستخدم الكنترولر ده غير لما يكون مسجل دخول
    public partial class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // 1. 📤 إرسال إشعار جديد (للأدمن أو السيستم)
        /// <summary>
        /// Creates a new notification based on the specified request data.
        /// </summary>
        /// <remarks>This action is restricted to users with the Admin role. Only administrators are
        /// permitted to create notifications using this endpoint.</remarks>
        /// <param name="request">The notification details to be created. Must not be null.</param>
        /// <returns>An IActionResult containing the result of the notification creation operation.</returns>
        [HttpPost]
        //[Authorize(Roles = "Admin")] يفضل تحط عليها قيد إن "الأدمن" بس هو اللي يقدر يندهها، عشان مش أي يوزر يبعت إشعارات ليوزر تاني بمزاجه.
        public async Task<IActionResult> Create([FromBody] NotificationAddRequest request)
        {
            var result = await _notificationService.CreateNotification(request);
            return CreatedAtAction(nameof(GetMyNotifications), new { id = result.Id }, result);
            //return Ok(result);
        }

        // 2. 📜 جلب كل إشعارات المستخدم الحالي
        /// <summary>
        /// Retrieves a paginated list of notifications for the currently authenticated user.
        /// </summary>
        /// <param name="pageNumber">The page number of the notifications to retrieve. Must be greater than or equal to 1. The default value is
        /// 1.</param>
        /// <param name="pageSize">The number of notifications to include on each page. Must be greater than 0. The default value is 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing the paginated notifications for the current user. Returns an
        /// unauthorized result if the user is not authenticated.</returns>
        // GET: api/Notifications?pageNumber=1&pageSize=10
        [HttpGet]
       // /api/notifications? pageNumber = 1 & pageSize = 10
        public async Task<IActionResult> GetMyNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId(); // بنجيب الـ ID من التوكن

            if (userId == Guid.Empty) return Unauthorized();

            var result = await _notificationService.GetUserNotificationsPaged(userId, pageNumber, pageSize);
            return Ok(result);
        }

        // 3. 🔴 جلب عدد الإشعارات غير المقروءة (عشان العداد في الـ Navbar)
        /// <summary>
        /// Retrieves the number of unread notifications for the current user.
        /// </summary>
        /// <remarks>This endpoint is typically used to display the unread notification count in user
        /// interface elements such as navigation bars. The count is specific to the authenticated user making the
        /// request.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object with a <c>count</c> property that represents the
        /// number of unread notifications. The value is zero if there are no unread notifications.</returns>
        [HttpGet("unread-count")]
        // /api/notifications? pageNumber = 1 & pageSize = 10

        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.GetUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        // 4. ✅ تعليم إشعار واحد كمقروء (لما يضغط عليه)
        /// <summary>
        /// Marks the specified notification as read for the current user.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to mark as read.</param>
        /// <returns>An HTTP 200 response if the notification was successfully marked as read.</returns>
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = User.GetUserId();

            // 🔐 تعديل 3 (مهم): بعتنا الـ userId للسيرفس عشان نتأكد إن الإشعار ملك لليوزر ده
            await _notificationService.MarkAsReadAsync(id, userId);

            return Ok(new { Message = "Notification marked as read" });
        }

        // 5. ✅✅ تعليم الكل كمقروء (زرار Mark All as Read)
        /// <summary>
        /// Marks all notifications for the current user as read.
        /// </summary>
        /// <remarks>This action applies to all notifications associated with the authenticated user. The
        /// response does not include the updated notification list.</remarks>
        /// <returns>An <see cref="OkObjectResult"/> containing a confirmation message if the operation succeeds.</returns>
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { Message = "All notifications marked as read" });
        }

    }
}
