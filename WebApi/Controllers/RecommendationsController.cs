using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.ServiceContracts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Retrieves personalized recommendations for the specified user.
        /// </summary>
        /// <remarks>The response includes a success flag, the user ID, the list of recommendations, and
        /// the total count. Returns a BadRequest if the user ID is invalid or if an exception is encountered during
        /// processing.</remarks>
        /// <param name="userId">The unique identifier of the user for whom to retrieve recommendations. Must not be an empty GUID.</param>
        /// <returns>An IActionResult containing the user's recommendations if found; otherwise, a BadRequest result if the user
        /// ID is invalid or an error occurs.</returns>
        [HttpGet("{userId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetRecommendations(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("Invalid user ID");

                // اجيب التوصيات من MongoDB لـ User المحدد
                var recommendations = await _recommendationService.GetUserRecommendationsAsync(userId);
                var recList = recommendations.ToList();

                return Ok(new
                {
                    success = true,
                    userId = userId,
                    data = recList,
                    count = recList.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}