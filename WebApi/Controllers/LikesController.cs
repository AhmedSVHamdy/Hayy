using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // POST: api/likes/toggle
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeDto dto)
        {
            // 1️⃣ الأمان: هات الـ ID من التوكن
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("لازم تسجل دخول الأول! 🔒");
            }

            // 2️⃣ املأ الـ DTO بالـ ID الحقيقي
            dto.UserId = Guid.Parse(userIdString);

            // 3️⃣ التحقق (مش محتاج if (!ModelState) لأن [ApiController] بيعملها)
            // بس الفاليديشن بتاع PostId لسه شغال أوتوماتيك

            try
            {
                var result = await _postLikeService.ToggleLikeAsync(dto);
                return Ok(result); // رجع الـ Response (عدد اللايكات الجديد وحالة اللايك)
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // لو البوست ممسوح
            }
            catch (Exception ex)
            {
                // سجل الخطأ هنا لو عندك Logger
                return StatusCode(500, "حدث خطأ غير متوقع.");
            }
        }
    }
}
