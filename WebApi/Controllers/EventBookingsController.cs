using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.ServiceContracts;
using System.Security.Claims;
using static Project.Core.DTO.CreateEventBooking;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // لازم اليوزر يكون عامل لوجين عشان يحجز
    public class EventBookingsController : ControllerBase
    {
        private readonly IEventBookingService _bookingService;

        public EventBookingsController(IEventBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        /// <summary>
        /// Creates a new booking for the authenticated user based on the provided booking details.
        /// </summary>
        /// <remarks>If the booking is waitlisted, the response includes the user's position in the
        /// waitlist. Upon successful booking, the user has 15 minutes to complete payment before the booking is
        /// cancelled. The user must be authenticated; otherwise, an unauthorized response is returned.</remarks>
        /// <param name="dto">The booking information to be used for creating the reservation. Must contain valid booking details; cannot
        /// be null.</param>
        /// <returns>An IActionResult containing the result of the booking operation. Returns a success message and booking data
        /// if the booking is created, a waitlist message if the booking is waitlisted, or a bad request message if the
        /// input is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            // بنجيب الـ UserId من التوكن بتاع الموبايل
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized(new { Message = "غير مصرح لك." });

            try
            {
                var result = await _bookingService.CreateBookingAsync(userId, dto);

                // لو اتحط في قائمة الانتظار نرجع رسالة مختلفة
                if (result.Status == "Waitlisted")
                {
                    return Ok(new
                    {
                        Message = $"تمت إضافتك لقائمة الانتظار في الترتيب رقم {result.WaitlistPosition}.",
                        Data = result
                    });
                }

                return Ok(new
                {
                    Message = "تم الحجز بنجاح. أمامك 15 دقيقة لإتمام الدفع وإلا سيتم إلغاء الحجز.",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Retrieves the list of bookings associated with the currently authenticated user.
        /// </summary>
        /// <remarks>This endpoint requires the user to be authenticated. The returned bookings are
        /// specific to the user identified by the authentication token.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the user's bookings if authentication is successful; otherwise, an
        /// unauthorized response.</returns>
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var result = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves the current user's booking for the specified event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event for which to retrieve the user's booking.</param>
        /// <returns>An <see cref="IActionResult"/> containing the booking details if found; <see cref="NotFoundResult"/> if the
        /// user has not booked this event; or <see cref="UnauthorizedResult"/> if the user is not authenticated.</returns>
        [HttpGet("my-bookings/{eventId}")]
        public async Task<IActionResult> GetMyBookingForEvent(Guid eventId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // هندور على حجز اليوزر ده في الإيفنت ده بالذات
            var result = await _bookingService.GetUserBookingForEventAsync(userId, eventId);

            if (result == null)
                return NotFound("أنت لم تقم بالحجز في هذا الحدث بعد.");

            return Ok(result);
        }

        [HttpPost("verify-ticket/{bookingId}")]
        public async Task<IActionResult> VerifyTicket(Guid bookingId)
        {
            // نجيب الـ ID بتاع المنظم اللي فاتح الأبلكيشن وبيمسح الكود
            var businessUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(businessUserIdString, out Guid businessUserId))
                return Unauthorized();

            var result = await _bookingService.VerifyTicketAsync(businessUserId, bookingId);

            if (result.IsValid)
                return Ok(result); // يرجع شاشة خضراء للمنظم
            else
                return BadRequest(result); // يرجع شاشة حمراء للمنظم وفيها رسالة التحذير
        }
    }
}
