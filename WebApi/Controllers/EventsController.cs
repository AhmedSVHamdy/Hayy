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
        private readonly IBusinessRepository _businessRepository;

        public EventsController(
            IEventService eventService,
            IPlaceRepository placeRepository,
            IBusinessRepository businessRepository)
        {
            _eventService = eventService;
            _placeRepository = placeRepository;
            _businessRepository = businessRepository;
        }
        /// <summary>
        /// Create a new event. This endpoint is accessible only to users with the "Business" role.
        /// </summary>
        /// <param name="dto">The event data to be created.</param>
        /// <returns>The created event.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        // 👈 التعديل الوحيد هنا: FromForm بدل FromBody
        public async Task<IActionResult> CreateEvent([FromForm] EventCreateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "غير مصرح لك. التوكن غير صالح." });
            }

            var business = await _businessRepository.GetBusinessByUserIdAsync(userId);
            if (business == null)
            {
                return StatusCode(403, new { Message = "حسابك غير مرتبط بأي بيزنس للقيام بهذه العملية." });
            }

            Guid actualBusinessId = business.Id;

            var place = await _placeRepository.GetByIdAsync(dto.PlaceId);
            if (place == null)
            {
                return NotFound(new { Message = "المكان المحدد غير موجود في النظام." });
            }

            if (place.BusinessId != actualBusinessId)
            {
                return StatusCode(403, new { Message = "عفواً، لا يمكنك إضافة حدث في مكان لا تملكه!" });
            }

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
        /// Get an event by its ID. This endpoint is accessible only to authenticated users.
        /// </summary>
        /// <param name="id">The ID of the event to retrieve.</param>
        /// <returns>The requested event.</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            var result = await _eventService.GetEventByIdAsync(id);
            if (result == null) return NotFound(new { Message = "الحدث غير موجود." });
            return Ok(result);
        }
        /// <summary>
        /// Get all active events. This endpoint is accessible to all users, including anonymous users.
        /// </summary>
        /// <returns>A list of all active events.</returns>
        [HttpGet("active")]
        [AllowAnonymous]
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
        /// Update an existing event. This endpoint is accessible only to users with the "Business" role.
        /// </summary>
        /// <param name="eventId">The ID of the event to update.</param>
        /// <param name="dto">The updated event data.</param>
        /// <returns>The updated event.</returns>
        [HttpPut("{eventId}")]
        [Authorize(Roles = "Business")]
        // 👈 التعديل هنا كمان: FromForm بدل FromBody
        public async Task<IActionResult> UpdateEvent(Guid eventId, [FromForm] UpdateEventDto dto)
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Delete an existing event. This endpoint is accessible only to users with the "Business" role.
        /// </summary>
        /// <param name="eventId">The ID of the event to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
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
                return NoContent();
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