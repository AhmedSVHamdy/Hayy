using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.ServiceContracts;
using System.Security.Claims;
using static Project.Core.DTO.CeratePlaceFollow;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceFollowsController : ControllerBase
    {
        private readonly IPlaceFollowService _placeFollowService;

        public PlaceFollowsController(IPlaceFollowService placeFollowService)
        {
            _placeFollowService = placeFollowService;
        }

        /// <summary>
        /// عمل متابعة أو إلغاء متابعة لمكان (Toggle)
        /// </summary>
        [HttpPost("toggle")]
        [Authorize] // لازم يكون مسجل دخول
        public async Task<IActionResult> ToggleFollow([FromBody] TogglePlaceFollowDto dto)
        {
            // بنجيب الـ ID بتاع اليوزر من التوكن
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "غير مصرح لك، برجاء تسجيل الدخول." });
            }

            try
            {
                var isFollowed = await _placeFollowService.ToggleFollowAsync(userId, dto);

                if (isFollowed)
                {
                    return Ok(new { Message = "تم متابعة المكان بنجاح ✅", IsFollowed = true });
                }
                else
                {
                    return Ok(new { Message = "تم إلغاء المتابعة ❌", IsFollowed = false });
                }
            }
            catch (KeyNotFoundException ex)
            {
                // لو المكان مش موجود في الداتابيز (اللي عملناها في السيرفس)
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // أي إيرور تاني
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// جلب كل المتابعين لمكان معين (بالباجينيشن)
        /// </summary>
        [HttpGet("place/{placeId}/followers")]
        // مش محتاجة Authorize لو مسموح لأي حد يشوف المتابعين، لو عايزها برايفت ضيفها
        public async Task<IActionResult> GetFollowersByPlaceId(Guid placeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _placeFollowService.GetFollowersByPlaceIdPagedAsync(placeId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// جلب كل الأماكن اللي اليوزر الحالي بيتابعها (بالباجينيشن)
        /// </summary>
        [HttpGet("user/follows")]
        [Authorize] // لازم يكون مسجل دخول عشان نعرف هو مين
        public async Task<IActionResult> GetFollowedPlacesByUser([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "غير مصرح لك، برجاء تسجيل الدخول." });
            }

            var result = await _placeFollowService.GetFollowedPlacesByUserIdPagedAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }
    }
}
