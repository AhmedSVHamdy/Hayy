using Microsoft.AspNetCore.Mvc;
using Project.Core.ServiceContracts;
using static Project.Core.DTO.CerateBusinessPostDto;
using Project.Core.Helpers;

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
        /// Creates a new post for the specified place using the provided post data.
        /// </summary>
        /// <remarks>The authenticated user's ID is automatically assigned to the post. The method
        /// requires a valid authentication token. The response includes a link to retrieve posts for the place
        /// associated with the created post.</remarks>
        /// <param name="dto">The data transfer object containing the details of the post to create. Must include valid post information;
        /// the user ID is assigned from the authenticated user's token.</param>
        /// <returns>An IActionResult indicating the outcome of the operation. Returns 201 Created with the created post if
        /// successful; 401 Unauthorized if the user token is invalid; 404 Not Found if the place does not exist; 403
        /// Forbidden if the user does not own the place; or 400 Bad Request for other errors.</returns>
        [HttpPost]
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

        // 2. عرض بوستات مكان معين (GET: api/BusinessPosts/{placeId})
        // بيدعم الـ Pagination
        /// <summary>
        /// Retrieves a paginated list of posts associated with the specified place.
        /// </summary>
        /// <remarks>Supports pagination to efficiently retrieve large sets of posts. Use the <paramref
        /// name="pageNumber"/> and <paramref name="pageSize"/> parameters to control paging.</remarks>
        /// <param name="placeId">The unique identifier of the place for which to retrieve posts.</param>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1. Defaults to 1.</param>
        /// <param name="pageSize">The number of posts to include in each page. Must be greater than 0. Defaults to 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing a paged result of posts for the specified place.</returns>
        [HttpGet("{placeId}")] // GET: api/BusinessPosts/{placeId}?pageNumber=1&pageSize=10
        public async Task<IActionResult> GetPlacePosts(Guid placeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // بننادي دالة السيرفس الجديدة اللي بترجع PagedResult
            var result = await _postService.GetPostsByPlaceIdPagedAsync(placeId, pageNumber, pageSize);

            return Ok(result);
        }
    }
}
