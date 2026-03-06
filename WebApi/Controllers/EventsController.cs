using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;
using Project.Infrastructure.Repositories;
using System.Security.Claims;
using static Project.Core.DTO.CreateEventDTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IPlaceRepository _placeRepository;
        private readonly IBusinessRepository _businessRepository; // 👈 1. ضفنا الريبو بتاع البيزنس

        public EventsController(
            IEventService eventService,
            IPlaceRepository placeRepository,
            IBusinessRepository businessRepository) // 👈 2. عملنا له Inject
        {
            _eventService = eventService;
            _placeRepository = placeRepository;
            _businessRepository = businessRepository;
        }
        

        /// <summary>
        /// Creates a new event for the business associated with the authenticated user.
        /// </summary>
        /// <remarks>This action is restricted to users in the 'Business' role. The event can only be
        /// created at a place owned by the authenticated business. The method validates ownership and existence of the
        /// place before creating the event.</remarks>
        /// <param name="dto">The event details to create, including the place identifier and event information. Must reference a place
        /// owned by the authenticated business user.</param>
        /// <returns>An HTTP response indicating the result of the event creation. Returns 201 Created with the event details if
        /// successful; 400 Bad Request if the input is invalid; 403 Forbidden if the user is not associated with a
        /// business or does not own the specified place; 404 Not Found if the place does not exist; 401 Unauthorized if
        /// the authentication token is invalid.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto dto)
        {
            // 1. نجيب الـ UserId بتاع اليوزر اللي عامل لوجين من التوكن (اللي هو d3e3f9a7...)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "غير مصرح لك. التوكن غير صالح." });
            }

            // 💡 2. السحر هنا: نجيب البيزنس المرتبط باليوزر ده من الداتابيز
            // (تأكد إن اسم الميثود دي مطابق للي عندك في الـ IBusinessRepository)
            var business = await _businessRepository.GetBusinessByUserIdAsync(userId);
            if (business == null)
            {
                return StatusCode(403, new { Message = "حسابك غير مرتبط بأي بيزنس للقيام بهذه العملية." });
            }

            // ده الـ BusinessId الحقيقي اللي إحنا عايزينه (اللي هو f2c77084...)
            Guid actualBusinessId = business.Id;

            // 3. نجيب المكان من الداتابيز
            var place = await _placeRepository.GetByIdAsync(dto.PlaceId);
            if (place == null)
            {
                return NotFound(new { Message = "المكان المحدد غير موجود في النظام." });
            }

            // 🛑 4. المقارنة العادلة: نقارن البيزنس بـ البيزنس!
            if (place.BusinessId != actualBusinessId)
            {
                return StatusCode(403, new { Message = "عفواً، لا يمكنك إضافة حدث في مكان لا تملكه!" });
            }

            // 5. كل حاجة تمام، نبعت للسيرفس تنفذ اللوجيك وتحفظ
            try
            {
                var result = await _eventService.CreateEventAsync(dto);
                return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Retrieves the event with the specified unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the event to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the event data if found; otherwise, a NotFound result.</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            var result = await _eventService.GetEventByIdAsync(id);
            if (result == null) return NotFound(new { Message = "الحدث غير موجود." });
            return Ok(result);
        }

        /// <summary>
        /// Handles HTTP GET requests to retrieve all currently active events. Allows anonymous access so that any user
        /// can view available events without authentication.
        /// </summary>
        /// <remarks>This endpoint does not require authentication and can be accessed by any user. The
        /// response format includes either the list of active events or a message if none are found.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the list of active events. If no events are available, returns a
        /// response with a message indicating that no events are currently available and an empty data set.</returns>
        [HttpGet("active")]
        [AllowAnonymous] // 👈 لو عايز أي حد يشوف الإيفنتات من غير ما يعمل لوجين
        public async Task<IActionResult> GetActiveEvents()
        {
            var result = await _eventService.GetActiveEventsAsync();

            if (result == null || !result.Any())
            {
                return Ok(new { Message = "لا توجد أحداث متاحة حالياً.", Data = result });
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates the specified event with new details provided by the business user.
        /// </summary>
        /// <remarks>This endpoint is restricted to users in the 'Business' role. The user must be
        /// authenticated and authorized to modify the specified event. Concurrency conflicts may occur if the event is
        /// modified simultaneously by multiple users.</remarks>
        /// <param name="eventId">The unique identifier of the event to update.</param>
        /// <param name="dto">An object containing the updated event information. Must include all required fields for the event.</param>
        /// <returns>An IActionResult indicating the outcome of the update operation. Returns 200 OK with the updated event on
        /// success; 404 Not Found if the event does not exist; 403 Forbidden if the user is not authorized; 409
        /// Conflict if a concurrency error occurs; or 400 Bad Request for other errors.</returns>
        [HttpPut("{eventId}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> UpdateEvent(Guid eventId, [FromBody] UpdateEventDto dto)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized("Token is invalid or missing User ID claim.");

            try
            {
                var result = await _eventService.UpdateEventAsync(eventId, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // مسك إيرور الـ Concurrency
            {
                return Conflict(new { message = ex.Message }); // 409 Conflict
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Deletes the specified event for the authenticated business user.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated as a user in the 'Business' role.
        /// The event will only be deleted if it belongs to the requesting user.</remarks>
        /// <param name="eventId">The unique identifier of the event to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation. Returns 204 No Content if the
        /// event is deleted successfully; 404 Not Found if the event does not exist; 403 Forbidden if the user is not
        /// authorized; or 400 Bad Request for other errors.</returns>
        [HttpDelete("{eventId}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> DeleteEvent(Guid eventId)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized("Token is invalid or missing User ID claim.");

            try
            {
                await _eventService.DeleteEventAsync(eventId, userId);
                return NoContent(); // 204
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
