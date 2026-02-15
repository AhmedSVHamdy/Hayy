using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.Security.Claims;
using static Project.Core.DTO.CeratePostComment;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly IPostCommentService _postCommentService;

        public CommentsController(IPostCommentService postCommentService)
        {
            _postCommentService = postCommentService;
        }
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto )
        {
            // 1️⃣ الأمان: هات الـ ID من التوكن
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("لازم تسجل دخول عشان تعلق! 🔒");
            }

            // 2️⃣ حط الـ ID في الـ DTO
            dto.UserId = Guid.Parse(userIdString);

            // 3️⃣ ابعت للسيرفس
            var result = await _postCommentService.AddCommentAsync(dto);

            // 4️⃣ رجع 201 (معناها تم الإنشاء بنجاح)
            // (CreateCommentDto مفيهوش Id للكومنت الجديد، فممكن نرجع Ok بالنتيجة وخلاص لو مش عامل Endpoint تجيب كومنت واحد)
            return Ok(result);
        }

        [HttpGet("{postId}")]
        [AllowAnonymous] // 🔓 عادي أي حد يشوف الكومنتات حتى لو مش مسجل
        public async Task<IActionResult> GetPostComments(Guid postId)
        {
            var comments = await _postCommentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }
    }
}
