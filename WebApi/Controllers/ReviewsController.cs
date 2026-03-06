using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Helpers;
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
        /// <summary>
        /// Creates a new review for a place using the provided review data.
        /// </summary>
        /// <remarks>Returns 400 Bad Request if the review cannot be added due to invalid operation, such
        /// as duplicate reviews. Returns 404 Not Found if the specified place does not exist. Returns 500 Internal
        /// Server Error for unexpected failures.</remarks>
        /// <param name="dto">The review information to be added. Must include valid place and rating details. The user identifier is set
        /// automatically based on the authenticated user.</param>
        /// <returns>An HTTP 201 Created response containing the newly created review if successful; otherwise, an appropriate
        /// error response such as 400 Bad Request or 404 Not Found.</returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {            

            try
            {
                // ✅ التعديل الصح: بنبعت الـ DTO للسيرفس وهي تتصرف
                dto.UserId = User.GetUserId();
                var result = await _reviewService.AddReviewAsync(dto);

                // بنرجع 201 Created مع الداتا الجديدة
                return CreatedAtAction(nameof(GetReviewsByPlace), new { placeId = result.PlaceId }, result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { Error = ex.Message }); } // عشان التكرار
            catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); } // عشان لو المكان مش موجود
            catch (Exception ex) { return StatusCode(500, new { Error = "حدث خطأ غير متوقع" }); }
        }

        // 2. 👇 الدالة الجديدة: عرض تقييمات مكان معين (GET api/reviews/{placeId})
        /// <summary>
        /// Retrieves a paginated list of reviews for the specified place.
        /// </summary>
        /// <remarks>This endpoint returns reviews in a paginated format. If the specified place has no
        /// reviews, the response will contain an empty list with status code 200 (OK).</remarks>
        /// <param name="placeId">The unique identifier of the place for which to retrieve reviews.</param>
        /// <param name="pageNumber">The page number of results to return. Must be greater than or equal to 1. Defaults to 1.</param>
        /// <param name="pageSize">The number of reviews to include per page. Must be greater than 0. Defaults to 10.</param>
        /// <returns>An <see cref="IActionResult"/> containing a list of reviews for the specified place. Returns an empty list
        /// if no reviews are found.</returns>
        [HttpGet("{placeId}")]
        [Authorize]
        // /api/reviews/{placeId}? pageNumber = 1 & pageSize = 10
        public async Task<IActionResult> GetReviewsByPlace(Guid placeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _reviewService.GetReviewsByPlaceIdPagedAsync(placeId, pageNumber, pageSize);

            // لو مفيش تقييمات ممكن ترجع ليستة فاضية عادي (Status 200)
            return Ok(reviews);
        }
        /// <summary>
        /// Updates an existing review with the specified data for the authenticated user.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated as a user with the 'User' role.
        /// The review can only be updated by its owner. The request must include a valid user token containing the User
        /// ID claim.</remarks>
        /// <param name="reviewId">The unique identifier of the review to update.</param>
        /// <param name="dto">An object containing the updated review information.</param>
        /// <returns>An IActionResult indicating the outcome of the update operation. Returns 200 (OK) with the updated review on
        /// success, 404 (Not Found) if the review does not exist, 403 (Forbidden) if the user is not authorized to
        /// update the review, or 400 (Bad Request) for other errors.</returns>
        [HttpPut("{reviewId}")]
        [Authorize(Roles = "User")] // مسموح لليوزر العادي بس
        public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdateReviewDto dto)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                // بنباصي الـ userId للسيرفس عشان نتأكد من الصلاحية
                var result = await _reviewService.UpdateReviewAsync(reviewId, dto, userId);
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
