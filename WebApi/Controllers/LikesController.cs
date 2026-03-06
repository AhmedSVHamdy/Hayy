using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;
using System.Security.Claims;
using static Project.Core.DTO.CeratePostLike;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly IPostLikeService _postLikeService;

        public LikesController(IPostLikeService postLikeService)
        {
            _postLikeService = postLikeService;
        }

       /// <summary>
       /// Toggles the like status for a post on behalf of the authenticated user.
       /// </summary>
       /// <remarks>The user must be authenticated to toggle the like status. The method will like the post
       /// if it is not already liked by the user, or unlike it if it is currently liked.</remarks>
       /// <param name="dto">An object containing the post identifier and any additional data required to toggle the like status.</param>
       /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK with the updated like
       /// status if successful; 401 Unauthorized if the user is not authenticated; 404 Not Found if the post does not
       /// exist; or 400 Bad Request if an error occurs.</returns>
        [HttpPost("toggle")] // POST api/likes/toggle
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeDto dto)
        {
            // 1️⃣ استخراج الـ ID من التوكن
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("يرجى تسجيل الدخول 🔒");
            }

            dto.UserId = userId;

            try
            {
                // السيرفس هتقوم بالواجب (Like Or Unlike)
                var result = await _postLikeService.ToggleLikeAsync(dto);

                // بنرجع 200 OK
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "حدث خطأ أثناء تسجيل الإعجاب.", details = ex.Message });
            }
        }
    }
}
