using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System.Security.Claims;

namespace WebApi.Controllers
{
    /// <summary>
    /// Web authentication controller for handling business and admin authentication operations.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for business/admin registration, login, email confirmation,
    /// password management, and admin creation. It is designed for web-based authentication flows.
    /// </remarks>
    [Route("api/web/auth")]
    [ApiController]
    public class WebAuthController : ControllerBase
    {
        private readonly IValidator<RegisterDTO> _registerDtoValidator;
        private readonly IConfiguration _configuration;
        private readonly IAuthWeb _authWeb;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAuthController"/> class.
        /// </summary>
        /// <param name="registerDtoValidator">The validator for registration DTOs.</param>
        /// <param name="authWeb">The web authentication service.</param>
        /// <param name="configuration">The application configuration.</param>
        public WebAuthController(
            IValidator<RegisterDTO> registerDtoValidator,
            IAuthWeb authWeb,
            IConfiguration configuration)
        {
            _registerDtoValidator = registerDtoValidator;
            _authWeb = authWeb;
            _configuration = configuration;
        }

        // =========================================================
        //  1. تسجيل Business (Updated ✅)
        // =========================================================
        /// <summary>
        /// Registers a new business account.
        /// </summary>
        /// <param name="registerDTO">The registration data for the business.</param>
        /// <param name="image">Optional profile image file for the business.</param>
        /// <returns>A <see cref="RegisterResponse"/> containing registration details and authentication token.</returns>
        /// <response code="200">Returns the registration response with user details and token.</response>
        /// <response code="400">If validation fails or registration data is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterBusiness([FromForm] RegisterDTO registerDTO, IFormFile? image)
        {
            // 1. الفاليديشن اليدوي
            ValidationResult validationResult = await _registerDtoValidator.ValidateAsync(registerDTO);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
                return BadRequest(new { Error = "Validation Failed", Details = errors });
            }

            try
            {
                // 👇 التغيير: السيرفيس بترجع RegisterResponse جاهز
                RegisterResponse response = await _authWeb.RegisterBusinessAsync(registerDTO, image);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
            }
        }

        // =========================================================
        //  2. تسجيل الدخول (Updated ✅)
        // =========================================================
        /// <summary>
        /// Authenticates a business or admin user.
        /// </summary>
        /// <param name="loginDTO">The login credentials containing email and password.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing user details and JWT token.</returns>
        /// <response code="200">Returns the authentication response with token and user information.</response>
        /// <response code="400">If the model state is invalid.</response>
        /// <response code="401">If credentials are invalid or user type is not authorized for web access.</response>
        /// <remarks>
        /// Only Business and Admin users can login through this endpoint. Customer users are rejected.
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // 👇 التغيير: السيرفيس بترجع AuthenticationResponse فيه التوكن والحالة
                AuthenticationResponse authResponse = await _authWeb.LoginAsync(loginDTO);

                // 🛡️ حماية إضافية: التأكد إن اللي داخل مش Customer (لأن ده Web API)
                if (authResponse.UserType != UserType.Business.ToString() &&
                    authResponse.UserType != UserType.Admin.ToString())
                {
                    return Unauthorized(new { Error = "Access denied. Only Business and Admin can login here." });
                }

                return Ok(authResponse);
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
        }

        // =========================================================
        //  3. تفعيل الإيميل
        // =========================================================
        /// <summary>
        /// Confirms a user's email address using the provided token.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="token">The email confirmation token sent to the user's email.</param>
        /// <returns>A success message if email is confirmed, otherwise an error response.</returns>
        /// <response code="200">Email confirmed successfully.</response>
        /// <response code="302">Redirects to frontend with error if userId or token is invalid.</response>
        /// <response code="400">If email confirmation fails.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return Redirect($"{frontendUrl}/login?status=error&message=invalid_link");
            }

            try
            {
                var result = await _authWeb.ConfirmEmailAsync(userId, token);

                if (result.Succeeded)
                {
                    // تحويل لصفحة النجاح في الفرونت
                    // return Redirect($"{frontendUrl}/login?status=success");
                    return Ok(new { Message = "Email confirmed successfully. You can login now." });
                }

                return BadRequest(new { Error = "Email confirmation failed", Details = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // =========================================================
        //  4. إعادة إرسال التفعيل
        // =========================================================
        /// <summary>
        /// Resends the email confirmation link to the specified email address.
        /// </summary>
        /// <param name="request">The request containing the email address.</param>
        /// <returns>A success message indicating the email was sent.</returns>
        /// <response code="200">Confirmation email sent successfully.</response>
        /// <response code="400">If the email is invalid or user not found.</response>
        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
        {
            try
            {
                await _authWeb.ResendConfirmationEmailAsync(request.Email);
                return Ok(new { Message = "Confirmation email sent successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
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
        //  6. LOGOUT
        // =========================================================
        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <returns>A success message if logout is successful.</returns>
        /// <response code="200">User logged out successfully.</response>
        /// <response code="400">If logout operation fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            bool isLoggedOut = await _authWeb.LogoutAsync(userId);
            if (!isLoggedOut) return BadRequest("Logout failed.");

            return Ok(new { Message = "Logged out successfully" });
        }

        // =========================================================
        //  7. CHANGE PASSWORD
        // =========================================================
        /// <summary>
        /// Changes the password for the currently authenticated user.
        /// </summary>
        /// <param name="request">The request containing current and new password.</param>
        /// <param name="validator">The validator for change password requests.</param>
        /// <returns>A success message if password is changed successfully.</returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">If validation fails or password change fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            [FromServices] IValidator<ChangePasswordRequest> validator)
        {
            var valResult = await validator.ValidateAsync(request);
            if (!valResult.IsValid) return BadRequest(valResult.ToDictionary());

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authWeb.ChangePasswordAsync(userId, request);

            if (!result.Succeeded)
            {
                return BadRequest(new { Error = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            return Ok(new { Message = "Password changed successfully. Please login again." });
        }

        // =========================================================
        //  8. FORGOT PASSWORD
        // =========================================================
        /// <summary>
        /// Initiates the password reset process by generating a reset token and sending it via email.
        /// </summary>
        /// <param name="request">The request containing the user's email address.</param>
        /// <param name="validator">The validator for forgot password requests.</param>
        /// <returns>A success message with optional test token for development.</returns>
        /// <response code="200">Password reset email sent if the email exists.</response>
        /// <response code="400">If validation fails.</response>
        /// <remarks>
        /// For security reasons, this endpoint always returns success even if the email doesn't exist.
        /// The TestToken should be removed in production environments.
        /// </remarks>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request,
            [FromServices] IValidator<ForgotPasswordRequest> validator)
        {
            var valResult = await validator.ValidateAsync(request);
            if (!valResult.IsValid) return BadRequest(valResult.ToDictionary());

            // استلام التوكن من السيرفيس
            var token = await _authWeb.GeneratePasswordResetTokenAsync(request.Email);

            // إرجاع التوكن للتيست (يمكن إزالته في الإنتاج)
            return Ok(new
            {
                Message = "If the email exists, a reset link has been sent.",
                TestToken = token
            });
        }

        // =========================================================
        //  9. RESET PASSWORD
        // =========================================================
        /// <summary>
        /// Resets a user's password using the provided reset token.
        /// </summary>
        /// <param name="request">The request containing email, token, and new password.</param>
        /// <param name="validator">The validator for reset password requests.</param>
        /// <returns>A success message if password is reset successfully.</returns>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">If validation fails or password reset fails.</response>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            [FromServices] IValidator<ResetPasswordRequest> validator)
        {
            var valResult = await validator.ValidateAsync(request);
            if (!valResult.IsValid) return BadRequest(valResult.ToDictionary());

            var result = await _authWeb.ResetPasswordAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(new { Error = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            return Ok(new { Message = "Password has been reset successfully. You can login now." });
        }
    }
}