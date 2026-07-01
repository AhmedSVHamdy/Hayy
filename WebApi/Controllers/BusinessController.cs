using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.Web.Controllers
{
    /// <summary>
    /// Controller for business-specific operations including onboarding and profile management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IValidator<BusinessOnboardingDTO> _onboardingValidator;
        // 💡 يفضل حقن ILogger هنا مستقبلاً لتسجيل الأخطاء

        public BusinessController(IBusinessService businessService, IValidator<BusinessOnboardingDTO> onboardingValidator)
        {
            _businessService = businessService;
            _onboardingValidator = onboardingValidator;
        }

        [HttpPost("onboarding")]
        [Authorize(Roles = "Business")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitOnboarding([FromForm] BusinessOnboardingDTO model)
        {
            // 1. Controller-Level Validation
            ValidationResult validationResult = await _onboardingValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { Success = false, Message = "Validation Failed", Errors = errors });
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // ✅ الحماية من الـ FormatException باستخدام TryParse
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { Success = false, Message = "Invalid or missing user token." });
                }

                await _businessService.SubmitBusinessDetailsAsync(userId, model);

                return Ok(new
                {
                    Success = true,
                    Message = "Details submitted successfully. Your account is under review."
                });
            }
            // ✅ التقاط الـ ValidationException القادم من الـ Service (إن وُجد)
            catch (ValidationException ex)
            {
                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { Success = false, Message = "Validation Failed", Errors = errors });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                // TODO: Log the error using ILogger here -> _logger.LogError(ex, "Error submitting onboarding");

                // ✅ إزالة Details = ex.Message لحماية الـ API من تسريب البيانات
                return StatusCode(500, new { Success = false, Message = "An internal server error occurred. Please try again later." });
            }
        }


        /// <summary>
        /// Gets the profile details and verification status of the current logged-in business.
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "Business")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BusinessProfileDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { Success = false, Message = "Invalid token." });
                }

                var profile = await _businessService.GetBusinessProfileByUserIdAsync(userId);
                return Ok(new { Success = true, Data = profile });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while fetching profile." });
            }
        }

        /// <summary>
        /// Updates the profile information (Brand name, Legal name, and optionally Logo).
        /// </summary>
        [HttpPut("profile")]
        [Authorize(Roles = "Business")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateBusinessProfileDTO model)
        {
            // التحقق الأساسي من القيم النصية
            if (string.IsNullOrWhiteSpace(model.BrandName) || string.IsNullOrWhiteSpace(model.LegalName))
            {
                return BadRequest(new { Success = false, Message = "Brand Name and Legal Name are required." });
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { Success = false, Message = "Invalid token." });
                }

                await _businessService.UpdateBusinessProfileAsync(userId, model);

                return Ok(new { Success = true, Message = "Profile updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while updating profile." });
            }
        }
        /// <summary>
        /// Gets the analytics/dashboard data for the current logged-in business.
        /// </summary>
        [HttpGet("analytics")]
        [Authorize(Roles = "Business")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BusinessAnalyticDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { Success = false, Message = "Invalid token." });
                }

                var analytics = await _businessService.GetMyAnalyticsAsync(userId);

                return Ok(new { Success = true, Data = analytics });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while fetching analytics." });
            }
        }

        /// <summary>
        /// Gets the email address of the user associated with a specific business ID.
        /// Only accessible by Admins for security/privacy.
        /// </summary>
        /// <param name="businessId">The unique identifier of the business.</param>
        [HttpGet("{businessId:guid}/user-email")]
        [Authorize(Roles = "Admin,Business")] // 🔒 تأمين الـ Endpoint للأدمن فقط لحماية الخصوصية
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BusinessUserEmailDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBusinessUserEmail([FromRoute] Guid businessId)
        {
            if (businessId == Guid.Empty)
            {
                return BadRequest(new { Success = false, Message = "Invalid Business ID." });
            }

            try
            {
                var result = await _businessService.GetBusinessUserEmailAsync(businessId);
                return Ok(new { Success = true, Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while fetching the email." });
            }
        }

        /// <summary>
        /// One-time script to sync and store all existing business analytics into the table.
        /// Accessible only by Admin.
        /// </summary>
        [HttpPost("sync-analytics")]
        [Authorize(Roles = "Admin")]
        private async Task<IActionResult> SyncAnalytics()
        {
            try
            {
                await _businessService.SyncAllBusinessesAnalyticsAsync();
                return Ok(new { Success = true, Message = "All existing business analytics have been calculated and stored successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}