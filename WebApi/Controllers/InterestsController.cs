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

