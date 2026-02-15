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
        private readonly IAuthUsers _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAuthController"/> class.
        /// </summary>
        /// <param name="registerDtoValidator">The validator for registration DTOs.</param>
        /// <param name="authWeb">The web authentication service.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="authService">The general authentication service for users.</param>
        public WebAuthController(
            IValidator<RegisterDTO> registerDtoValidator,
            IAuthWeb authWeb,
            IConfiguration configuration,
            IAuthUsers authService)
        {
            _registerDtoValidator = registerDtoValidator;
            _authWeb = authWeb;
            _configuration = configuration;
            _authService = authService;
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
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // 1. حدد رابط الفرونت إند بتاعك (سواء لوكال أو برودكشن)
            // لو أنت شغال React محلياً غالباً بيكون البورت 3000
            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                // لو البيانات ناقصة، رجعه لصفحة اللوجين مع رسالة خطأ
                return Redirect($"{frontendUrl}/login?status=error&message=invalid_link");
            }

            try
            {
                var result = await _authWeb.ConfirmEmailAsync(userId, token);

                if (result.Succeeded)
                {
                    // ✅ الصح: وجه المستخدم لصفحة اللوجين في الفرونت إند مع رسالة نجاح
                    return Redirect($"{frontendUrl}/login?status=success");
                }

                // لو فشل التفعيل
                return Redirect($"{frontendUrl}/login?status=error&message=confirmation_failed");
            }
            catch (Exception )
            {
                // لو حصل خطأ في السيرفر
                return Redirect($"{frontendUrl}/login?status=error&message=server_error");
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        // =========================================================
        //  10. MAKE ADMIN (Promote User to Admin)
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


        // =========================================================
        //  11. REFRESH TOKEN (New ✅)
        // =========================================================
        /// <summary>
        /// Refreshes the JWT token using an expired access token and a valid refresh token.
        /// </summary>
        /// <param name="tokenDTO">The DTO containing the expired AccessToken and the RefreshToken.</param>
        /// <returns>A new <see cref="AuthenticationResponse"/> with new tokens.</returns>
        /// <response code="200">Returns the new tokens.</response>
        /// <response code="400">If the tokens are invalid or expired.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (tokenDTO == null)
            {
                return BadRequest(new { Error = "Invalid client request" });
            }

            try
            {
                // استدعاء السيرفيس الخاصة بالويب
                var response = await _authWeb.RefreshTokenAsync(tokenDTO);

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
        //  12. GOOGLE LOGIN (WEB)
        // =========================================================
        /// <summary>
        /// Authenticates a business user using Google OAuth credentials for web access.
        /// </summary>
        /// <param name="socialDto">The DTO containing Google authentication token and user information.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing user details and JWT token.</returns>
        /// <response code="200">Returns the authentication response with token and user information.</response>
        /// <response code="400">If the Google token is invalid or authentication fails.</response>
        /// <remarks>
        /// This endpoint registers the user as a Business type if they don't exist, or logs them in if they do.
        /// Only Business accounts can use Google login through the web interface.
        /// </remarks>
        [HttpPost("google-login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleLogin([FromBody] SocialLoginDTO socialDto)
        {
            // 👇 هنا بنجبره يتسجل كـ Business
            var response = await _authService.GoogleLoginAsync(socialDto, UserType.Business.ToString());
            return Ok(response);
        }

        // =========================================================
        //  13. CHECK EMAIL EXISTS
        // =========================================================
        /// <summary>
        /// Checks if an email is already registered in the system.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns>An object indicating whether the email exists.</returns>
        /// <response code="200">Returns true if email exists, otherwise false.</response>
        /// <response code="400">If email parameter is missing or invalid.</response>
        /// <remarks>
        /// This endpoint is useful for client-side validation during registration to provide
        /// immediate feedback to users about email availability.
        /// </remarks>
        [HttpGet("check-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { Error = "Email is required" });

            bool exists = await _authWeb.CheckEmailExistsAsync(email);

            // بنرجع Object عشان الفرونت إند يقدر يقرأه بسهولة JSON
            return Ok(new { exists = exists });
        }

        // =========================================================
        //  14. REVOKE TOKEN (Logout from specific device)
        // =========================================================
        /// <summary>
        /// Revokes a specific refresh token, effectively logging out from a specific device.
        /// </summary>
        /// <param name="request">The request containing the refresh token to revoke.</param>
        /// <returns>A success message if the token is revoked successfully.</returns>
        /// <response code="200">Token revoked successfully.</response>
        /// <response code="400">If token is missing, invalid, or not found.</response>
        /// <remarks>
        /// This endpoint allows users to invalidate a specific refresh token, which is useful for
        /// logging out from a specific device or session. The token can be provided in the request
        /// body or in cookies.
        /// </remarks>
        [HttpPost("revoke-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            // بنشوف لو التوكن جاي في الـ Body ولا في الـ Cookies
            var token = request.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { Error = "Token is required" });

            var result = await _authWeb.RevokeTokenAsync(token);

            if (!result)
                return BadRequest(new { Error = "Token is invalid or not found" });

            return Ok(new { Message = "Token revoked successfully" });
        }

        // =========================================================
        //  15. GET USER PROFILE
        // =========================================================
        /// <summary>
        /// Retrieves the profile details of the currently logged-in user.
        /// </summary>
        /// <returns>A <see cref="UserProfileDTO"/> containing the user's profile information.</returns>
        /// <response code="200">Returns the user profile data successfully.</response>
        /// <response code="400">If user not found or an error occurs.</response>
        /// <response code="401">If the user is not authenticated or token is invalid.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            // بنجيب الـ ID من التوكن
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var userProfile = await _authWeb.GetUserProfileAsync(userId);
                return Ok(userProfile);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}