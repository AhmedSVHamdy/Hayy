using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // 👈 ضفناها هنا
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        /// <summary>
        /// Add a new review. Only authenticated users can access this endpoint.
        /// </summary>
        /// <param name="dto">The review data to be added.</param>
        /// <returns>The created review.</returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        // 👈 التعديل الوحيد هنا: استخدمنا [FromForm] بدل [FromBody]
        public async Task<IActionResult> AddReview([FromForm] CreateReviewDto dto)
        {
            try
            {
                dto.UserId = User.GetUserId();
                var result = await _reviewService.AddReviewAsync(dto);

                return CreatedAtAction(nameof(GetReviewsByPlace), new { placeId = result.PlaceId }, result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { Error = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { Error = "حدث خطأ غير متوقع" }); }
        }
        /// <summary>
        /// Get reviews by place ID with pagination. This endpoint is accessible to all authenticated users.
        /// </summary>
        /// <param name="placeId">The ID of the place to get reviews for.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The number of reviews per page.</param>
        /// <returns>A paged list of reviews for the specified place.</returns>
        [HttpGet("{placeId}")]
        [Authorize]
        public async Task<IActionResult> GetReviewsByPlace(Guid placeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _reviewService.GetReviewsByPlaceIdPagedAsync(placeId, pageNumber, pageSize);
            return Ok(reviews);
        }

        /// <summary>
        /// Get reviews by user ID with pagination. Only the user themselves or an admin can access this endpoint.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetReviewsByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByUserIdPagedAsync(userId, pageNumber, pageSize);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "حدث خطأ غير متوقع" });
            }
        }
        /// <summary>
        /// Update a review by its ID. Only the user who created the review can update it.
        /// </summary>
        /// <param name="reviewId">The ID of the review to update.</param>
        /// <param name="dto">The updated review data.</param>
        /// <returns>The updated review.</returns>
        [HttpPut("{reviewId}")]
        [Authorize(Roles = "User")]
        // 👈 التعديل هنا كمان: استخدمنا [FromForm] بدل [FromBody]
        public async Task<IActionResult> UpdateReview(Guid reviewId, [FromForm] UpdateReviewDto dto)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Token is invalid or missing User ID claim.");
            }

            try
            {
                var result = await _reviewService.UpdateReviewAsync(reviewId, dto, userId);
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
    }
}