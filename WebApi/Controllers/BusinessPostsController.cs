using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {

            // 1. هات الـ ID بتاع اليوزر اللي عامل Login حالياً
            // (ClaimTypes.NameIdentifier) دي الستاندرد اللي شايلة الـ ID
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userIdString == null)
            {
                return Unauthorized("لازم تسجل دخول الأول! 🔒");
            }

            // 2. حط الـ ID ده جوه الـ DTO عشان السيرفس تشوفه
            dto.UserId = Guid.Parse(userIdString);
            try
            {
                var result = await _postService.CreatePostAsync(dto);
                return CreatedAtAction(nameof(GetPlacePosts), new { placeId = dto.PlaceId }, result);
            }
            catch (UnauthorizedAccessException ex) // لو طلع مش صاحب المطعم
            {
                return StatusCode(403, new { message = ex.Message }); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{placeId}")]
        public async Task<IActionResult> GetPlacePosts(Guid placeId)
        {
            var posts = await _postService.GetPostsByPlaceIdAsync(placeId);
            return Ok(posts);
        }
    }
}
