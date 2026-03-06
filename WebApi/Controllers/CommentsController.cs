using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.Helpers;
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
        /// <summary>
        /// Creates a new comment for a post using the specified comment data.
        /// </summary>
        /// <remarks>The user must be authenticated to add a comment. The user identifier is automatically
        /// set based on the current user's authentication token.</remarks>
        /// <param name="dto">The data transfer object containing the details of the comment to add. Must include the comment content and
        /// the associated post identifier.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the operation. Returns a 200 OK response with
        /// the created comment data if successful, or a 401 Unauthorized response if the user is not authenticated.</returns>
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto )
        {
            // 1️⃣ الأمان: هات الـ ID من التوكن
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("جلسة العمل انتهت، يرجى تسجيل الدخول.");
            }

            dto.UserId = userId;

            try
            {
                var result = await _postCommentService.AddCommentAsync(dto);
                // بنرجع 200 OK مع النتيجة عشان الفرونت يعرض الكومنت فوراً
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Retrieves a paginated list of comments for the specified post.
        /// </summary>
        /// <param name="postId">The unique identifier of the post for which to retrieve comments.</param>
        /// <param name="pageNumber">The page number of results to retrieve. Must be greater than or equal to 1. The default is 1.</param>
        /// <param name="pageSize">The number of comments to include on each page. Must be greater than 0. The default is 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing a paged result of comments for the specified post.</returns>
        [HttpGet("{postId}")] // GET api/comments/{postId}?pageNumber=1&pageSize=10
        [AllowAnonymous] // 🔓 عادي أي حد يشوف الكومنتات حتى لو مش مسجل
        public async Task<IActionResult> GetPostComments(Guid postId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // بننادي السيرفس اللي بترجع PagedResult
            var result = await _postCommentService.GetCommentsByPostIdPagedAsync(postId, pageNumber, pageSize);

            return Ok(result);
        }
    }
}
