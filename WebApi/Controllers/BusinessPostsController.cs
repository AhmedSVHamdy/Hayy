using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Project.Core.DTO.CerateBusinessPostDto;
using static Project.Core.DTO.CeratePostComment;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessPostsController : ControllerBase
    {
        private readonly IBusinessPostService _postService;

        public BusinessPostsController(IBusinessPostService postService)
        {
            _postService = postService;
        }
        /// <summary>
        /// Creates a new post. Only users with the "Business" role can perform this action.
        /// </summary>
        /// <param name="dto">The post data to be created.</param>
        /// <returns>The created post.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            var userId = User.GetUserId();

            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                dto.UserId = userId;
                var result = await _postService.CreatePostAsync(dto);

                return CreatedAtAction(nameof(GetPlacePosts), new { placeId = result.PlaceId }, result);
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
        /// <summary>
        /// Retrieves posts for a specific place with pagination. This endpoint is accessible to any authenticated user.
        /// </summary>
        /// <param name="placeId">The ID of the place for which to retrieve posts.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The number of posts per page.</param>
        /// <returns>A paginated list of posts for the specified place.</returns>
        [HttpGet("{placeId}")] // GET: api/BusinessPosts/{placeId}?pageNumber=1&pageSize=10
        [Authorize]
        public async Task<IActionResult> GetPlacePosts(Guid placeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetPostsByPlaceIdPagedAsync(placeId, pageNumber, pageSize);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves a specific post by its ID. This endpoint is accessible to any authenticated user.
        /// </summary>
        /// <param name="postId">The ID of the post to retrieve.</param>
        /// <returns>The requested post.</returns>
        [HttpGet("post/{postId}")] // GET: api/BusinessPosts/post/{postId}
        [Authorize]
        public async Task<IActionResult> GetPostById(Guid postId)
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(postId);

                if (post == null)
                    return NotFound(new { message = "البوست غير موجود" });

                return Ok(post);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Updates an existing post. Only users with the "Business" role can perform this action.
        /// </summary>
        /// <param name="postId">The ID of the post to update.</param>
        /// <param name="dto">The updated post data.</param>
        /// <returns>The updated post.</returns>
        [HttpPut("{postId}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> UpdatePost(Guid postId, [FromForm] UpdatePostDto dto)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                var result = await _postService.UpdatePostAsync(postId, dto, userId);
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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Deletes a post by its ID. Only users with the "Business" role can perform this action.
        /// </summary>
        /// <param name="postId">The ID of the post to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{postId}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                await _postService.DeletePostAsync(postId, userId);
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
        /// <summary>
        /// Retrieves all posts with pagination. This endpoint is accessible to any authenticated user.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of posts per page.</param>
        /// <returns>A paginated list of posts.</returns>
        [HttpGet("all")] // GET: api/BusinessPosts/all?pageNumber=1&pageSize=50
        [Authorize]
        public async Task<IActionResult> GetAllPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _postService.GetAllPostsPagedAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Retrieves comments for a specific post. This endpoint is accessible to any authenticated user.
        /// </summary>
        /// <param name="postId">The ID of the post for which to retrieve comments.</param>
        /// <returns>A list of comments for the specified post.</returns>
        [HttpGet("{postId}/comments")]
        [Authorize]
        public async Task<IActionResult> GetPostComments(Guid postId)
        {
            try
            {
                var comments = await _postService.GetPostCommentsAsync(postId);
                return Ok(comments);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Replies to a comment on a post. Only users with the "Business" role can perform this action.
        /// </summary>
        /// <param name="commentId">The ID of the comment to reply to.</param>
        /// <param name="request">The reply content.</param>
        /// <returns>The created reply.</returns>
        [HttpPost("{commentId}/reply")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> ReplyToComment(Guid commentId, [FromBody] ReplyToCommentRequest request)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == Guid.Empty)
                    return Unauthorized("Token is invalid or missing User ID claim.");

                var dto = new ReplyCommentDto
                {
                    CommentId = commentId,
                    UserId = userId,
                    Content = request.Content
                };

                var reply = await _postService.ReplyToCommentAsync(dto);
                return Ok(new { success = true, data = reply });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class ReplyToCommentRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}