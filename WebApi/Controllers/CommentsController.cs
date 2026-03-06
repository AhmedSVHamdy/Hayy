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
        [Authorize(Roles = "User")]
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
        [Authorize]
        public async Task<IActionResult> GetPostComments(Guid postId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // بننادي السيرفس اللي بترجع PagedResult
            var result = await _postCommentService.GetCommentsByPostIdPagedAsync(postId, pageNumber, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing comment with new content provided by the user.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated as a user. The user can only
        /// update their own comments. If the user session has expired, the request will be rejected with a 401
        /// Unauthorized response.</remarks>
        /// <param name="commentId">The unique identifier of the comment to update.</param>
        /// <param name="dto">An object containing the updated comment data. Must not be null.</param>
        /// <returns>An IActionResult indicating the result of the update operation. Returns 200 OK with the updated comment on
        /// success, 404 Not Found if the comment does not exist, 403 Forbidden if the user is not authorized, 401
        /// Unauthorized if the user session has expired, or 400 Bad Request for other errors.</returns>
        [HttpPut("{commentId}")]
        [Authorize(Roles = "User")] // لازم يكون يوزر
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentDto dto)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("جلسة العمل انتهت، يرجى تسجيل الدخول.");
            }

            try
            {
                // بنبعت الـ UserId للسيرفس عشان تتأكد من الصلاحية
                var result = await _postCommentService.UpdateCommentAsync(commentId, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message }); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); // 400
            }
        }

        /// <summary>
        /// Deletes the specified comment if it belongs to the currently authenticated user.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and in the 'User' role. Only the
        /// owner of the comment can delete it. If the user's session has expired, an unauthorized response is
        /// returned.</remarks>
        /// <param name="commentId">The unique identifier of the comment to delete.</param>
        /// <returns>An HTTP 204 No Content response if the comment is successfully deleted; 404 Not Found if the comment does
        /// not exist; 403 Forbidden if the user is not authorized to delete the comment; or 400 Bad Request for other
        /// errors.</returns>
        [HttpDelete("{commentId}")]
        [Authorize(Roles = "User")] // لازم يكون يوزر
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("جلسة العمل انتهت، يرجى تسجيل الدخول.");
            }

            try
            {
                // بنبعت الـ UserId للسيرفس عشان تتأكد إنه بيمسح الكومنت بتاعه بس
                await _postCommentService.DeleteCommentAsync(commentId, userId);

                return NoContent(); // 204 No Content (الرد الصحيح لنجاح الحذف)
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
