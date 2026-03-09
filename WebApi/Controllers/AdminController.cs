using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using Project.Core.Services;
using System.Security.Claims;

namespace Project.Web.Controllers
{
    /// <summary>
    /// Controller for administrative operations including dashboard statistics, audit logs, and business verification management.
    /// </summary>
    /// <remarks>
    /// All endpoints in this controller require Admin role authorization.
    /// </remarks>
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")] // 🔒 حماية كاملة لكل الـ Endpoints
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IBusinessService _businessService;
        private readonly IValidator<ReviewBusinessDTO> _reviewValidator; // 👇 حقن الفاليديشن هنا
        private readonly IValidator<RegisterDTO> _registerDtoValidator;
        private readonly IAuthWeb _authWeb;


        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="adminService">Service for handling admin-specific operations.</param>
        /// <param name="businessService">Service for handling business-related operations.</param>
        /// <param name="reviewValidator">Validator for business review operations.</param>
        public AdminController(
            IAdminService adminService,
            IBusinessService businessService,
            IValidator<ReviewBusinessDTO> reviewValidator,
            IValidator<RegisterDTO> registerDtoValidator,
            IAuthWeb authWeb)
        {
            _adminService = adminService;
            _businessService = businessService;
            _reviewValidator = reviewValidator;
            _registerDtoValidator = registerDtoValidator;
            _authWeb = authWeb;
        }

        // =========================================================
        //  1. DASHBOARD STATS
        // =========================================================
        /// <summary>
        /// Retrieves comprehensive dashboard statistics for the admin panel.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing dashboard statistics including metrics and counts.
        /// </returns>
        /// <response code="200">Returns the dashboard statistics successfully.</response>
        /// <response code="401">Unauthorized - User is not authenticated or lacks Admin role.</response>
        [HttpGet("dashboard-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _adminService.GetDashboardStatisticsAsync();
            return Ok(new { Success = true, Data = stats });
        }

        // =========================================================
        //  2. AUDIT LOG
        // =========================================================
        /// <summary>
        /// Retrieves the complete audit log of administrative actions.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the list of all audit log entries.
        /// </returns>
        /// <response code="200">Returns the audit log successfully.</response>
        /// <response code="401">Unauthorized - User is not authenticated or lacks Admin role.</response>
        [HttpGet("audit-log")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAuditLog()
        {
            var logs = await _adminService.GetAuditLogAsync();
            return Ok(new { Success = true, Data = logs });
        }

        // =========================================================
        //  3. GET PENDING VERIFICATIONS (نقلناها هنا ✅)
        // =========================================================
        /// <summary>
        /// Retrieves all businesses that are pending verification.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the list of businesses awaiting admin review.
        /// </returns>
        /// <response code="200">Returns the list of pending businesses successfully.</response>
        /// <response code="401">Unauthorized - User is not authenticated or lacks Admin role.</response>
        [HttpGet("verifications/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPendingVerifications()
        {
            // بننادي السيرفيس الخاصة بالبيزنس لأنها المسؤولة عن الداتا دي
            var result = await _businessService.GetPendingVerificationsAsync();
            return Ok(new { Success = true, Data = result });
        }

        // =========================================================
        //  4. REVIEW BUSINESS (Approve / Reject) (نقلناها هنا ✅)
        // =========================================================
        /// <summary>
        /// Reviews a business verification request and approves or rejects it.
        /// </summary>
        /// <param name="businessId">The unique identifier of the business to review.</param>
        /// <param name="reviewDto">The review decision containing approval status and optional admin notes.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the review operation.
        /// </returns>
        /// <response code="200">Business reviewed successfully.</response>
        /// <response code="400">Bad request - Validation failed or business cannot be reviewed.</response>
        /// <response code="401">Unauthorized - User is not authenticated or lacks Admin role.</response>
        /// <response code="404">Business not found.</response>
        /// <remarks>
        /// This endpoint validates the review data, extracts the admin ID from the authenticated user claims,
        /// and processes the business verification decision.
        /// </remarks>
        [HttpPost("verifications/review/{businessId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReviewBusiness(Guid businessId, [FromBody] ReviewBusinessDTO reviewDto)
        {
            // 1. Validation
            ValidationResult validationResult = await _reviewValidator.ValidateAsync(reviewDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { Success = false, Message = "Validation Failed", Errors = errors });
            }

            try
            {
                // 2. Get Admin ID
                var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (adminIdClaim == null) return Unauthorized();

                var adminId = Guid.Parse(adminIdClaim);

                // 3. Perform Review
                await _businessService.ReviewBusinessAsync(businessId, reviewDto, adminId);

                string statusMessage = reviewDto.IsApproved ? "approved" : "rejected";
                return Ok(new
                {
                    Success = true,
                    Message = $"Business has been {statusMessage} successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        // =========================================================
        //  5. إنشاء أدمن (Updated ✅)
        // =========================================================
        /// <summary>
        /// Creates a new admin user account. Requires admin authorization.
        /// </summary>
        /// <param name="registerDTO">The registration data for the new admin.</param>
        /// <param name="image">Optional profile image file for the admin.</param>
        /// <returns>A <see cref="RegisterResponse"/> containing the new admin details.</returns>
        /// <response code="200">Admin created successfully with registration details.</response>
        /// <response code="400">If validation fails or registration data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have admin role.</response>
        [HttpPost("create-admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAdmin([FromForm] RegisterDTO registerDTO, IFormFile? image)
        {
            ValidationResult validationResult = await _registerDtoValidator.ValidateAsync(registerDTO);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
                return BadRequest(new { Error = "Validation Failed", Details = errors });
            }

            try
            {
                // 👇 التغيير: استخدام Response الجديد
                RegisterResponse response = await _authWeb.RegisterAdminAsync(registerDTO, image);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // =========================================================
        //  6. MAKE ADMIN (Promote User to Admin)
        // =========================================================
        /// <summary>
        /// Promotes an existing user to Admin role.
        /// </summary>
        /// <param name="email">The email of the user to be promoted to Admin.</param>
        /// <returns>A success message if the user is promoted successfully.</returns>
        /// <response code="200">User promoted to Admin successfully.</response>
        /// <response code="400">If email is missing, user not found, or already an admin.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have admin role.</response>
        /// <response code="500">If an internal server error occurs.</response>
        /// <remarks>
        /// This endpoint requires Admin role. Only super admins can promote users to admin status.
        /// </remarks>
        [HttpPost("make-admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MakeAdmin(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            try
            {
                var result = await _authWeb.MakeAdmin(email);

                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = $"User {email} has been successfully promoted to Admin."
                    });
                }

                return BadRequest("Failed to promote user.");
            }
            catch (ArgumentException ex)
            {
                // لو المستخدم مش موجود أو هو أدمن أصلاً
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }
        }
    }
}