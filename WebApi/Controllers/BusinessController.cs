using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.Security.Claims;

namespace Project.Web.Controllers
{
    /// <summary>
    /// Controller for business-specific operations including onboarding and profile management.
    /// </summary>
    /// <remarks>
    /// This controller handles operations exclusive to business users, primarily the business
    /// onboarding process where businesses submit their details for admin verification.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IValidator<BusinessOnboardingDTO> _onboardingValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessController"/> class.
        /// </summary>
        /// <param name="businessService">The service for handling business operations.</param>
        /// <param name="onboardingValidator">The validator for business onboarding data.</param>
        public BusinessController(IBusinessService businessService, IValidator<BusinessOnboardingDTO> onboardingValidator)
        {
            _businessService = businessService;
            _onboardingValidator = onboardingValidator;
        }

        /// <summary>
        /// Submits business onboarding details for admin verification.
        /// </summary>
        /// <param name="model">The business onboarding data including documents and images.</param>
        /// <returns>A success response if the submission is valid and processed.</returns>
        /// <response code="200">Business details submitted successfully and awaiting admin review.</response>
        /// <response code="400">If validation fails, required documents are missing, or business already exists.</response>
        /// <response code="401">If the user is not authenticated or lacks Business role.</response>
        /// <response code="500">If an internal server error occurs during submission.</response>
        /// <remarks>
        /// This endpoint requires the user to have the "Business" role. The submitted data will be
        /// validated and stored for admin review. Required documents include business registration
        /// certificates and identification documents. The business account will be marked as
        /// "PendingVerification" until an admin approves or rejects it.
        /// </remarks>
        [HttpPost("onboarding")]
        [Authorize(Roles = "Business")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitOnboarding([FromForm] BusinessOnboardingDTO model)
        {
            ValidationResult validationResult = await _onboardingValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                // توحيد شكل الـ Errors عشان الفرونت إند ميتلخبطش
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { Success = false, Message = "Validation Failed", Errors = errors });
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                var userId = Guid.Parse(userIdClaim);

                await _businessService.SubmitBusinessDetailsAsync(userId, model);

                return Ok(new
                {
                    Success = true,
                    Message = "Details submitted successfully. Your account is under review."
                });
            }
            catch (ArgumentException ex) // 👈 ضفنا دي عشان مشاكل الصور الناقصة
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex) // 👈 عشان لو عنده بيزنس قبل كدا
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the error here (ex)
                return StatusCode(500, new { Success = false, Message = "An internal error occurred.", Details = ex.Message });
            }
        }
    }
}