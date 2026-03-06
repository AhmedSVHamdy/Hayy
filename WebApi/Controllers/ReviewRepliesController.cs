using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;
using static Project.Core.DTO.CreateReviewReplyDTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ReviewRepliesController : ControllerBase
    {
        private readonly IReviewReplyService _service;

        public ReviewRepliesController(IReviewReplyService service)
        {
            _service = service;
        }
        /// <summary>
        /// Adds a reply to an existing review using the specified reply data.
        /// </summary>
        /// <remarks>The user must be authenticated to add a reply. The replier's user ID is automatically
        /// set based on the authenticated user.</remarks>
        /// <param name="dto">The data transfer object containing the details of the reply to add. Must include the review identifier and
        /// reply content.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the operation. Returns a 200 OK response with
        /// the created reply data if successful.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> AddReply([FromBody] CreateReviewReplyDto dto)
        {
            dto.ReplierId = User.GetUserId(); // بنجيب الـ ID من التوكن
            var result = await _service.AddReplyAsync(dto);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves a paginated list of replies associated with the specified review.
        /// </summary>
        /// <param name="reviewId">The unique identifier of the review for which to retrieve replies.</param>
        /// <param name="page">The page number of the results to retrieve. Must be greater than or equal to 1. The default value is 1.</param>
        /// <param name="size">The number of replies to include per page. Must be greater than 0. The default value is 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing the paginated list of replies for the specified review.</returns>
        [HttpGet("{reviewId}")] // GET: api/ReviewReplies/{reviewId}?page=1&size=10
        [Authorize]
        public async Task<IActionResult> GetReplies(Guid reviewId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _service.GetRepliesByReviewIdAsync(reviewId, page, size);
            return Ok(result);
        }
        /// <summary>
        /// Deletes the specified reply for the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the reply to delete.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the delete operation. Returns a 204 No Content
        /// response if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> DeleteReply(Guid id)
        {
            var userId = User.GetUserId();
            await _service.DeleteReplyAsync(id, userId);
            return NoContent();
        }
        
        /// <summary>
        /// Updates an existing review reply with new content provided by the business user.
        /// </summary>
        /// <remarks>This action is restricted to users in the "Business" role. The user must be
        /// authenticated and authorized to update the specified reply. The reply must exist and belong to the
        /// authenticated business user.</remarks>
        /// <param name="replyId">The unique identifier of the reply to update.</param>
        /// <param name="dto">An object containing the updated reply content and related information.</param>
        /// <returns>An IActionResult indicating the result of the update operation. Returns 200 OK with the updated reply on
        /// success, 404 Not Found if the reply does not exist, 403 Forbidden if the user is not authorized, 401
        /// Unauthorized if the user ID is missing or invalid, or 400 Bad Request for other errors.</returns>
        [HttpPut("{replyId}")]
        [Authorize(Roles = "Business")] // مسموح للبيزنس بس
        public async Task<IActionResult> UpdateReply(Guid replyId, [FromBody] UpdateReviewReplyDto dto)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                // بنباصي الـ userId للسيرفس عشان التأكد من الصلاحية
                var result = await _service.UpdateReplyAsync(replyId, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message }); // 403
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); // 400
            }
        }
    }
}
