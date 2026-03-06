using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.ServiceContracts;
using Project.Infrastructure.Repositories;
using System.Security.Claims;
using static Project.Core.DTO.CreateEventDTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 حماية البوابة الأولى

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

        [HttpPost]
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
    }
}
