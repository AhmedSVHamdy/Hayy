using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.Security.Claims;

namespace WebApi.Controllers
{
    /// <summary>
    /// Mobile application authentication controller for handling customer authentication operations.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for mobile app users (customers) including registration, login,
    /// email confirmation via deep links, password management, account deletion, and social login integration.
    /// It is specifically designed for mobile application authentication flows.
    /// </remarks>
    [Route("api/app/auth")]
    [ApiController]
    public class AppAuthController : ControllerBase
    {
        private readonly IAuthUsers _authService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppAuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service for user operations.</param>
        /// <param name="configuration">The application configuration.</param>
        public AppAuthController(IAuthUsers authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        // =========================
        // 1. REGISTER
        // =========================
        /// <summary>
        /// Registers a new customer account for the mobile application.
        /// </summary>
        /// <param name="registerDTO">The registration data for the customer.</param>
        /// <param name="image">Optional profile image file for the customer.</param>
        /// <returns>A <see cref="RegisterResponse"/> containing registration details and authentication token.</returns>
        /// <response code="200">Returns the registration response with user details and token.</response>
        /// <response code="400">If model validation fails or registration data is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterDTO registerDTO, IFormFile? image)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var response = await _authService.Register(registerDTO, image);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Details = ex.Message });
            }
        }

        // =========================
        // 2. LOGIN
        // =========================
        /// <summary>
        /// Authenticates a customer user for the mobile application.
        /// </summary>
        /// <param name="loginDTO">The login credentials containing email and password.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing user details and JWT token.</returns>
        /// <response code="200">Returns the authentication response with token and user information.</response>
        /// <response code="401">If credentials are invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                AuthenticationResponse response = await _authService.Login(loginDTO);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
            }
        }

        // =========================
        // 3. REFRESH TOKEN
        // =========================
        /// <summary>
        /// Refreshes the JWT token using an expired access token and a valid refresh token.
        /// </summary>
        /// <param name="tokenModel">The DTO containing the expired AccessToken and the RefreshToken.</param>
        /// <returns>A new <see cref="AuthenticationResponse"/> with new tokens.</returns>
        /// <response code="200">Returns the new access and refresh tokens.</response>
        /// <response code="400">If the tokens are invalid or expired.</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDTO tokenModel)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(tokenModel);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // =========================
        // 4. CONFIRM EMAIL (Deep Link Redirect)
        // =========================
        /// <summary>
        /// Confirms a user's email address and redirects to the mobile app via deep link.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="token">The email confirmation token sent to the user's email.</param>
        /// <returns>An HTML page that redirects to the mobile app with confirmation status.</returns>
        /// <response code="200">Returns HTML content with deep link redirect.</response>
        /// <response code="400">If userId or token parameters are invalid.</response>
        /// <remarks>
        /// This endpoint is designed for mobile deep linking. It returns an HTML page that automatically
        /// redirects to the mobile app using the configured URL scheme (e.g., hayy://confirm-email).
        /// </remarks>
        [HttpGet("confirm-email-redirect")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailRedirect(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid parameters");

            // تفعيل الإيميل
            var result = await _authService.ConfirmEmailAsync(userId, token);
            string status = result.Succeeded ? "success" : "error";

            var scheme = _configuration["AppSettings:MobileScheme"] ?? "hayy";

            // تكوين الرابط: hayy://confirm-email?...
            var appDeepLink = $"{scheme}://confirm-email?status=success&userId={userId}";

            // صفحة HTML بسيطة للتحويل (أضمن من Redirect المباشر في بعض المتصفحات)
            return Content($@"
                <html>
                    <body>
                        <p>Redirecting to app...</p>
                        <script>window.location.href = '{appDeepLink}';</script>
                        <a href='{appDeepLink}'>Click here if not redirected</a>
                    </body>
                </html>", "text/html");
        }

        // =========================
        // 5. FORGOT PASSWORD
        // =========================
        /// <summary>
        /// Initiates the password reset process by generating a reset token and sending it via email.
        /// </summary>
        /// <param name="request">The request containing the user's email address.</param>
        /// <returns>A success message indicating the reset link was sent if the email exists.</returns>
        /// <response code="200">Returns success message (always returns success for security).</response>
        /// <response code="500">If an internal server error occurs.</response>
        /// <remarks>
        /// For security reasons, this endpoint always returns success even if the email doesn't exist.
        /// </remarks>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _authService.GeneratePasswordResetTokenAsync(request.Email);
                return Ok(new { Message = "If email exists, reset link sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // =========================
        // 6. RESET PASSWORD REDIRECT
        // =========================
        /// <summary>
        /// Redirects the user to the mobile app for password reset via deep link.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="token">The password reset token.</param>
        /// <returns>An HTML page that redirects to the mobile app with reset parameters.</returns>
        /// <response code="200">Returns HTML content with deep link redirect to reset password screen.</response>
        /// <remarks>
        /// This endpoint is designed to be accessed from email links and redirects to the mobile app
        /// using a deep link containing the email and reset token.
        /// </remarks>
        [HttpGet("reset-password-redirect")]
        [AllowAnonymous]
        public IActionResult ResetPasswordRedirect(string email, string token)
        {
            var mobileDeepLink = $"hayy://reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            // نفس فكرة صفحة الـ HTML اللي أنت عاملها (ممتازة)
            var htmlContent = $@"
                <html>
                    <head><title>Reset Password</title></head>
                    <body>
                        <h2>Opening App...</h2>
                        <script>window.location.href = '{mobileDeepLink}';</script>
                    </body>
                </html>";

            return Content(htmlContent, "text/html");
        }

        // =========================
        // 7. RESET PASSWORD (Final Step)
        // =========================
        /// <summary>
        /// Resets a user's password using the provided reset token.
        /// </summary>
        /// <param name="request">The request containing email, token, and new password.</param>
        /// <returns>A success message if password is reset successfully.</returns>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">If validation fails or password reset fails.</response>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(request);

            if (result.Succeeded)
                return Ok(new { Message = "Password reset successfully." });

            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        // =========================
        // 8. CHANGE PASSWORD
        // =========================
        /// <summary>
        /// Changes the password for the currently authenticated user.
        /// </summary>
        /// <param name="request">The request containing current and new password.</param>
        /// <returns>A success message if password is changed successfully.</returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">If validation fails or password change fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            var result = await _authService.ChangePasswordAsync(userId!, request);

            if (!result.Succeeded)
                return BadRequest(new { Error = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new { Message = "Password changed successfully." });
        }

        // =========================
        // 9. LOGOUT
        // =========================
        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <returns>A success message if logout is successful.</returns>
        /// <response code="200">User logged out successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _authService.Logout(userId);
            return Ok(new { Message = "Logged out successfully" });
        }

        // =========================
        // 10. RESEND CONFIRMATION
        // =========================
        /// <summary>
        /// Resends the email confirmation link to the specified email address.
        /// </summary>
        /// <param name="request">The request containing the email address.</param>
        /// <returns>A success message indicating the email was sent.</returns>
        /// <response code="200">Confirmation email sent successfully.</response>
        /// <response code="400">If the email is invalid or user not found.</response>
        [HttpPost("resend-confirmation-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
        {
            try
            {
                await _authService.ResendConfirmationEmailAsync(request.Email!);
                return Ok(new { Message = "Sent" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // =========================
        // 11. GET CURRENT USER PROFILE
        // =========================
        /// <summary>
        /// Retrieves the profile details of the currently logged-in user.
        /// </summary>
        /// <returns>A <see cref="UserProfileDTO"/> containing the user's profile information.</returns>
        /// <response code="200">Returns the user profile data successfully.</response>
        /// <response code="401">If the user is not authenticated or token is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Error = "Invalid Token" });

            try
            {
                UserProfileDTO userProfile = await _authService.GetUserProfileAsync(userId);
                return Ok(userProfile);
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
            }
        }

        // =========================
        // 12. DELETE ACCOUNT
        // =========================
        /// <summary>
        /// Permanently deletes the currently authenticated user's account.
        /// </summary>
        /// <returns>A success message if account deletion is successful.</returns>
        /// <response code="200">Account deleted successfully.</response>
        /// <response code="400">If account deletion fails or user not found.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="500">If an internal server error occurs.</response>
        /// <remarks>
        /// This operation is irreversible and will permanently delete all user data.
        /// </remarks>
        [HttpDelete("account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var result = await _authService.DeleteAccountAsync(userId);

                if (!result)
                {
                    return BadRequest(new { Error = "Failed to delete account or user not found." });
                }

                return Ok(new { Message = "Account deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
            }
        }

        // =========================
        // 13. GOOGLE LOGIN
        // =========================
        /// <summary>
        /// Authenticates a user using Google OAuth credentials for the mobile application.
        /// </summary>
        /// <param name="socialDto">The DTO containing Google authentication token and user information.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing user details and JWT token.</returns>
        /// <response code="200">Returns the authentication response with token and user information.</response>
        /// <response code="400">If the Google token is invalid or authentication fails.</response>
        /// <remarks>
        /// This endpoint registers the user as a Customer type if they don't exist, or logs them in if they do.
        /// </remarks>
        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] SocialLoginDTO socialDto)
        {
            // هيدخل هنا كـ User عادي
            var response = await _authService.GoogleLoginAsync(socialDto);
            return Ok(response);
        }
    }
}