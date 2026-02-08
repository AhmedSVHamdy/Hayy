using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.Security.Claims;

namespace Project.Web.Controllers // أو حسب الـ Namespace بتاع مشروع الـ API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IValidator<BusinessOnboardingDTO> _onboardingValidator;
        private readonly IValidator<ReviewBusinessDTO> _reviewValidator;

        public BusinessController(IBusinessService businessService, IValidator<BusinessOnboardingDTO> onboardingValidator, IValidator<ReviewBusinessDTO> reviewValidator)
        {
            _businessService = businessService;
            _onboardingValidator = onboardingValidator;
            _reviewValidator = reviewValidator;
        }

        // =========================================================
        //  1. SUBMIT BUSINESS DETAILS (Onboarding)
        //  Role: Business Only
        //  Content-Type: multipart/form-data (عشان الصور)
        // =========================================================
        [HttpPost("onboarding")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> SubmitOnboarding([FromForm] BusinessOnboardingDTO model)
        {
            ValidationResult validationResult = await _onboardingValidator.ValidateAsync(model);

            // 3. فحص النتيجة
            if (!validationResult.IsValid)
            {
                // هنا تقدر تشكل الرد براحتك
                // مثلاً نرجع قائمة بالأخطاء فقط
                var errors = validationResult.Errors
                    .Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage })
                    .ToList();

                return BadRequest(new { Success = false, Errors = errors });
            }
            try
            {
                // استخراج الـ UserId من التوكن
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
            catch (InvalidOperationException ex)
            {
                // مثلاً لو اليوزر عنده بيزنس بالفعل
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // أي خطأ آخر (مثل فشل رفع الصور)
                return StatusCode(500, new { Error = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

    }
}