using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;
using static Project.Core.DTO.CerateBusinessPostDto;

namespace WebApi.Controllers
{
    /// <summary>
    /// Handles HTTP requests related to post creation and retrieval for specific places.
    /// </summary>
    /// <remarks>This controller provides endpoints for creating new posts and retrieving posts associated
    /// with a particular place. It is intended to be used as part of a RESTful API and is configured with attribute
    /// routing under the 'api/[controller]' route. All actions require valid input models and return appropriate HTTP
    /// status codes based on the operation outcome.</remarks>
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
        /// Creates a new post for the authenticated business user.
        /// </summary>
        /// <remarks>This action requires authentication and the user must have the 'Business' role. The
        /// user ID is automatically set based on the authenticated user and does not need to be provided in the request
        /// body.</remarks>
        /// <param name="dto">The data transfer object containing the details of the post to create. Must not be null.</param>
        /// <returns>A 201 Created response with the created post if successful; otherwise, an appropriate error response such as
        /// 400 Bad Request, 401 Unauthorized, 403 Forbidden, or 404 Not Found.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }
            // 2. حط الـ ID ده جوه الـ DTO عشان السيرفس تشوفه
            try
            {
                // ✅ الطريقة الاحترافية: بنجيب الـ ID من التوكن مباشرة
                // بدلاً من الكود اليدوي الطويل
                dto.UserId = userId;

                var result = await _postService. CreatePostAsync(dto);

                // بنرجع 201 Created مع الرابط اللي يجيب البوستات
                return CreatedAtAction(nameof(GetPlacePosts), new { placeId = result.PlaceId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                // لو المكان مش موجود (404)
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // لو اليوزر مش صاحب المكان (403)
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // أي خطأ تاني (400)
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a paginated list of posts associated with the specified place.
        /// </summary>
        /// <param name="placeId">The unique identifier of the place for which to retrieve posts.</param>
        /// <param name="pageNumber">The page number of results to retrieve. Must be greater than or equal to 1. Defaults to 1.</param>
        /// <param name="pageSize">The number of posts to include in each page of results. Must be greater than 0. Defaults to 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing a paged result of posts for the specified place.</returns>
        [HttpGet("{placeId}")] // GET: api/BusinessPosts/{placeId}?pageNumber=1&pageSize=10
        [Authorize]
        public async Task<IActionResult> GetPlacePosts(Guid placeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // بننادي دالة السيرفس الجديدة اللي بترجع PagedResult
            var result = await _postService.GetPostsByPlaceIdPagedAsync(placeId, pageNumber, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing post with new data provided by the business user.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated as a user in the 'Business' role.
        /// The user must own the post to perform the update.</remarks>
        /// <param name="postId">The unique identifier of the post to update.</param>
        /// <param name="dto">An object containing the updated post data. Cannot be null.</param>
        /// <returns>An IActionResult indicating the result of the update operation. Returns 200 OK with the updated post data if
        /// successful; 404 Not Found if the post does not exist; 403 Forbidden if the user does not own the post; or
        /// 400 Bad Request for other errors.</returns>
        [HttpPut("{postId}")]
        [Authorize(Roles = "Business")] // لازم يكون صاحب مكان
        public async Task<IActionResult> UpdatePost(Guid postId, [FromBody] UpdatePostDto dto)
        {
            var userId = User.GetUserId(); // بنجيب الـ ID بتاع اليوزر من التوكن
            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                // بنبعت البوست المترتب عليه التعديل، وبنبعت معاه الـ userId عشان السيرفس تتأكد إنه صاحب المكان
                var result = await _postService.UpdatePostAsync(postId, dto, userId);
                return Ok(result); // بيرجع 200 OK مع الداتا الجديدة
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 لو البوست أو المكان مش موجود
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message }); // 403 لو اليوزر بيحاول يعدل بوست مش بتاعه
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); // 400 لأي خطأ تاني
            }
        }

        /// <summary>
        /// Deletes the specified post owned by the authenticated business user.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated as a user in the 'Business' role.
        /// Only the owner of the post can delete it. If the post does not exist, a 404 Not Found response is returned.
        /// If the user is not authorized to delete the post, a 403 Forbidden response is returned.</remarks>
        /// <param name="postId">The unique identifier of the post to delete.</param>
        /// <returns>An HTTP 204 No Content response if the post is successfully deleted; otherwise, an appropriate error
        /// response such as 401 Unauthorized, 403 Forbidden, 404 Not Found, or 400 Bad Request.</returns>
        [HttpDelete("{postId}")]
        [Authorize(Roles = "Business")] // لازم يكون صاحب مكان
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            var userId = User.GetUserId(); // بنجيب الـ ID بتاع اليوزر من التوكن
            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                // بنبعت الـ postId اللي هيتمسح، والـ userId عشان نتأكد من الصلاحية
                await _postService.DeletePostAsync(postId, userId);

                return NoContent(); // بيرجع 204 No Content (وده الرد القياسي في الـ API لما الحذف ينجح)
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 لو البوست ممسوح أصلاً
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message }); // 403 لو بيحاول يمسح بوست مش بتاعه
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); // 400 لأي خطأ تاني
            }
        }
    }
}
