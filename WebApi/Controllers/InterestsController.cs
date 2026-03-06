using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/interests")]
    [ApiController]

    public class InterestsController : ControllerBase
    {
        private readonly IInterestService _interestService;
        private readonly UserManager<User> _userManager;

        // حقن السيرفس والـ UserManager
        public InterestsController(IInterestService interestService, UserManager<User> userManager)
        {
            _interestService = interestService;
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves the list of all available interests.
        /// </summary>
        /// <remarks>Use this endpoint to obtain all interests currently stored in the system. The
        /// response will be empty if no interests exist.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the list of interests with status code 200 (OK) if interests are
        /// found; status code 204 (No Content) if no interests are available; or status code 500 (Internal Server
        /// Error) if an error occurs.</returns>
        [HttpGet]
        public async Task<IActionResult> GetInterestsList()
        {
            try
            {
                var result = await _interestService.GetAllInterestsAsync();

                if (result == null || !result.Any())
                {
                    return NoContent(); 
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving interests", Details = ex.Message });
            }
        }

        /// <summary>
        /// Saves the user's interests based on the provided request data.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated. The user's identity is determined
        /// from the authentication token. The request model must pass validation; otherwise, a bad request response is
        /// returned.</remarks>
        /// <param name="request">The request object containing the user's selected interests. Cannot be null. The request body must conform
        /// to the expected model schema.</param>
        /// <returns>An IActionResult indicating the result of the operation. Returns 200 OK if the interests are saved
        /// successfully, 400 Bad Request if the request data is invalid, 401 Unauthorized if the user is not
        /// authenticated, or 500 Internal Server Error if an unexpected error occurs.</returns>
        [HttpPost]
        public async Task<IActionResult> SaveInterests([FromBody] UserInterestRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. الحصول على الـ User الحالي من التوكن
                // (User property is available inside Controller because of [Authorize])
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized(new { Message = "User not found" });
                }

                // 2. استدعاء السيرفس (التي ستتحدث مع الريبوزيتوري)
                // السيرفس هي اللي هتعالج منطق الـ Skip والقيم الافتراضية
                await _interestService.SaveUserInterestsAsync(user.Id, request);

                return Ok(new { Message = "Interests saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error saving interests", Details = ex.Message });
            }
        }
    }
}

