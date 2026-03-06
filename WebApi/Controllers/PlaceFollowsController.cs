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
        /// Toggles the follow status of the specified place for the authenticated user.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated. The follow status is toggled: if
        /// the user is currently following the place, they will be unfollowed, and vice versa.</remarks>
        /// <param name="dto">An object containing the details of the place to follow or unfollow.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a 200 OK response with a
        /// message and the new follow status if successful; 401 Unauthorized if the user is not authenticated; 404 Not
        /// Found if the specified place does not exist; or 400 Bad Request for other errors.</returns>
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
        /// Retrieves a paginated list of followers for the specified place.
        /// </summary>
        /// <param name="placeId">The unique identifier of the place whose followers are to be retrieved.</param>
        /// <param name="pageNumber">The page number of the results to return. Must be greater than or equal to 1. The default is 1.</param>
        /// <param name="pageSize">The maximum number of followers to include in a single page of results. Must be greater than 0. The default
        /// is 10.</param>
        /// <returns>An IActionResult containing a paginated list of followers for the specified place.</returns>
        [HttpGet("place/{placeId}/followers")]
        // مش محتاجة Authorize لو مسموح لأي حد يشوف المتابعين، لو عايزها برايفت ضيفها
        public async Task<IActionResult> GetFollowersByPlaceId(Guid placeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _placeFollowService.GetFollowersByPlaceIdPagedAsync(placeId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of places followed by the currently authenticated user.
        /// </summary>
        /// <remarks>This endpoint requires the user to be authenticated. The results are specific to the
        /// currently logged-in user and are returned in a paginated format based on the provided parameters.</remarks>
        /// <param name="pageNumber">The page number of the results to retrieve. Must be greater than or equal to 1. The default value is 1.</param>
        /// <param name="pageSize">The number of items to include on each page. Must be greater than 0. The default value is 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing a paginated list of followed places for the authenticated user.
        /// Returns an unauthorized response if the user is not authenticated.</returns>
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
