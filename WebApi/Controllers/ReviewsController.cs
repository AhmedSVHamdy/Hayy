using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    /// <summary>
    /// Handles HTTP requests related to product reviews, including creating new reviews.
    /// </summary>
    /// <remarks>This controller provides endpoints for managing reviews in the application. It is configured
    /// as an API controller and uses dependency injection to access review-related services and object mapping
    /// functionality. All routes are prefixed with 'api/reviews'.</remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        // شيلنا الـ IMapper من هنا لأننا مش محتاجينه، السيرفس هي اللي بتعمل المابينج
        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // 1. إضافة تقييم جديد (POST api/reviews)
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {            

            try
            {
                // ✅ التعديل الصح: بنبعت الـ DTO للسيرفس وهي تتصرف
                var result = await _reviewService.AddReviewAsync(dto);

                // بنرجع 201 Created مع الداتا الجديدة
                return CreatedAtAction(nameof(GetReviewsByPlace), new { placeId = result.PlaceId }, result);
            }
            catch (Exception ex)
            {
                // لو حصل خطأ (مثلاً اليوزر مش موجود أو المكان غلط)
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 2. 👇 الدالة الجديدة: عرض تقييمات مكان معين (GET api/reviews/{placeId})
        [HttpGet("{placeId}")]
        public async Task<IActionResult> GetReviewsByPlace(Guid placeId)
        {
            var reviews = await _reviewService.GetReviewsByPlaceIdAsync(placeId);

            // لو مفيش تقييمات ممكن ترجع ليستة فاضية عادي (Status 200)
            return Ok(reviews);
        }
    }
}
