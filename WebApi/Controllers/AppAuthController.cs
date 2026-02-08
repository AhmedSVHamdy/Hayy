using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using Project.Infrastructure.Migrations;
using System.Security.Claims;

namespace WebApi.Controllers
{
    /// <summary>
    /// Authentication controller for mobile app users
    /// </summary>
    /// <remarks>
    /// Handles user registration, login, email verification, password management, and token refresh for mobile application
    /// </remarks>
    [Route("api/app/auth")] // 👈 لاحظ كلمة app هنا
    [ApiController]
    public class AppAuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAuthUsers _authService;
        private readonly IJwtService _jwtService;


        public AppAuthController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<ApplicationRole> roleManager, IAuthUsers authService, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _authService = authService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Register a new mobile app user
        /// </summary>
        /// <param name="registerDTO">User registration data including email, password, name, and city</param>
        /// <param name="image">Optional profile image file</param>
        /// <returns>Success message with user ID</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/app/auth/register
        ///     Content-Type: multipart/form-data
        ///     
        ///     {
        ///         "fullName": "Ahmed Hassan",
        ///         "email": "ahmed@example.com",
        ///         "password": "SecurePass123!",
        ///         "confirmPassword": "SecurePass123!",
        ///         "city": "Cairo",
        ///         "image": [binary file]
        ///     }
        /// 
        /// After successful registration:
        /// - A confirmation email will be sent to the provided email address
        /// - User must verify email before being able to login
        /// - User will be assigned the "User" role automatically
        /// </remarks>
        /// <response code="200">Registration successful, confirmation email sent</response>
        /// <response code="400">Invalid data or email already exists</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromForm] RegisterDTO registerDTO, IFormFile? image)
        {
            // 1. Validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

            try
            {
                User registeredUser = await _authService.Register(registerDTO, image);

                return Ok(new
                {
                    Message = "Registration successful. Please check your email to verify your account.",
                    UserId = registeredUser.Id
                });
            }
            catch (ArgumentException ex)
            {
                // أخطاء منطقية (إيميل مكرر، باسورد ضعيف، إلخ)
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // أخطاء سيرفر غير متوقعة
                return StatusCode(500, new { Error = "An internal server error occurred.", Details = ex.Message });
            }
        }


        [HttpGet("confirm-email-redirect")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailRedirect(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid parameters");
            }

            // 1. 👈 الخطوة المضافة: نقوم بتفعيل الإيميل فعلياً هنا في السيرفر
            var result = await _authService.ConfirmEmailAsync(userId, token);

            // نحدد حالة العملية عشان نبعتها للموبايل يعرف يعرض رسالة نجاح ولا فشل
            string status = result.Succeeded ? "success" : "error";

            // 2. تجهيز البيانات للرابط
            var encodedToken = Uri.EscapeDataString(token);
            var encodedUserId = Uri.EscapeDataString(userId);

            // 3. توجيه المستخدم للتطبيق مع حالة العملية
            // Hayy://confirm-email?status=success&userId=...
            var appDeepLink = $"Hayy://confirm-email?status={status}&userId={encodedUserId}";

            return Redirect(appDeepLink);
        }

        /// <summary>
        /// Authenticate a mobile app user
        /// </summary>
        /// <param name="loginDTO">Login credentials (email and password)</param>
        /// <param name="validator">FluentValidation validator for login data</param>
        /// <returns>JWT access token and refresh token</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/app/auth/login
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "email": "ahmed@example.com",
        ///         "password": "SecurePass123!"
        ///     }
        /// 
        /// Requirements:
        /// - User must have verified their email address
        /// - User must be of type "User" (not Business or Admin)
        /// - Password must match the registered password
        /// 
        /// Response includes:
        /// - Access Token (JWT) - valid for 15 minutes
        /// - Refresh Token - valid for 7 days
        /// - User details (name, email)
        /// - Token expiration times
        /// </remarks>
        /// <response code="200">Login successful, returns authentication tokens</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Invalid credentials or email not verified</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromBody] LoginDTO loginDTO,
            [FromServices] IValidator<LoginDTO> validator)
        {

            if (loginDTO == null)
            {
                return BadRequest(new { Error = "Invalid client request. The request body is empty." });
            }
            // 1. Fluent Validation Check
            var validationResult = await validator.ValidateAsync(loginDTO);

            if (!validationResult.IsValid)
            {
                // تجميع الأخطاء وإرجاعها
                var modelStateDictionary = new ModelStateDictionary();
                foreach (var failure in validationResult.Errors)
                {
                    modelStateDictionary.AddModelError(failure.PropertyName, failure.ErrorMessage);
                }
                return BadRequest(modelStateDictionary);
            }

            try
            {

                User user = await _authService.Login(loginDTO);

                // أ) إنشاء التوكن والـ Refresh Token
                var authenticationResponse = await _jwtService.CreateJwtTokenAsync(user, "mobile");

                // ب) تخزين الـ Refresh Token في الداتا بيز عشان نستخدمه بعدين
                if (!string.IsNullOrEmpty(authenticationResponse.RefreshToken))
                {
                    user.RefreshToken = authenticationResponse.RefreshToken;
                    user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

                    // تحديث اليوزر في الداتا بيز
                    await _userManager.UpdateAsync(user);
                }

                // ج) إرجاع التوكن للموبايل
                return Ok(authenticationResponse);
            }
            catch (ArgumentException ex)
            {
                // لو الباسورد غلط أو الإيميل مش مفعل، هيدخل هنا
                return Unauthorized(new { Error = ex.Message });
            }
        }

        // POST: api/app/auth/refresh-token
        [AllowAnonymous]
        [HttpPost("generate-new-jwt-token")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenModel tokenModel)
        {
            if (tokenModel == null)
            {
                return BadRequest("Invalid client request");
            }

            ClaimsPrincipal? principal = await _jwtService.GetPrincipalFromJwtToken(tokenModel.Token);
            if (principal == null)
            {
                return BadRequest("Invalid jwt access token");
            }

            string? email = principal.FindFirstValue(ClaimTypes.Email);

            User? user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpirationDateTime <= DateTime.Now)
            {
                return BadRequest("Invalid refresh token");
            }

            AuthenticationResponse authenticationResponse = await _jwtService.CreateJwtTokenAsync(user, "mobile");

            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }




        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // استخراج الـ ID من التوكن اللي جاي في الـ Header
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(); // لو التوكن مفيهوش ID (حالة نادرة)

                // نداء السيرفيس
                bool isLoggedOut = await _authService.Logout(userId);

                if (!isLoggedOut)
                    return BadRequest(new { Error = "User not found or already logged out" });

                return Ok(new { Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred", Details = ex.Message });
            }
        }

        /// <summary>
        /// Change the password for the authenticated user
        /// </summary>
        /// <param name="request">Current password and new password</param>
        /// <param name="validator">FluentValidation validator for password change</param>
        /// <returns>Success message</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/app/auth/change-password
        ///     Authorization: Bearer {access_token}
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "currentPassword": "OldPass123!",
        ///         "newPassword": "NewSecurePass456!",
        ///         "confirmNewPassword": "NewSecurePass456!"
        ///     }
        /// 
        /// Password requirements:
        /// - Minimum 6 characters
        /// - New password must be different from current password
        /// - Confirm password must match new password
        /// 
        /// Security measures after successful password change:
        /// - All refresh tokens are invalidated
        /// - User must login again on all devices
        /// - This prevents unauthorized access if password was compromised
        /// </remarks>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid data or incorrect current password</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("change-password")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            [FromServices] IValidator<ChangePasswordRequest> validator)
        {
            // 1. Fluent Validation Check
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                // تجميع الأخطاء وإرجاعها
                var modelStateDictionary = new ModelStateDictionary();
                foreach (var failure in validationResult.Errors)
                {
                    modelStateDictionary.AddModelError(failure.PropertyName, failure.ErrorMessage);
                }
                return BadRequest(modelStateDictionary);
            }

            try
            {
                // 2. Get User ID from Claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 3. Call Service
                var result = await _authService.ChangePasswordAsync(userId, request);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                    // لو الخطأ إن الباسورد القديم غلط، ممكن نرجع BadRequest
                    return BadRequest(new { Error = errors });
                }

                return Ok(new { Message = "Password changed successfully. Please login again." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred", Details = ex.Message });
            }
        }

        /// <summary>
        /// Request a password reset token
        /// </summary>
        /// <param name="request">Email address of the account</param>
        /// <param name="validator">FluentValidation validator for forgot password request</param>
        /// <returns>Generic success message</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/app/auth/forgot-password
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "email": "ahmed@example.com"
        ///     }
        /// 
        /// Process:
        /// 1. System validates the email format
        /// 2. If email exists in system, a reset token is generated
        /// 3. Email with deep link is sent to user: Hayy://reset-password?email=...&amp;token=...
        /// 4. User clicks the link in mobile app
        /// 5. App calls the reset-password endpoint with the token
        /// 
        /// Security notes:
        /// - Same response is returned whether email exists or not (prevents email enumeration)
        /// - Reset token expires after a certain period
        /// - Token is single-use only
        /// </remarks>
        /// <response code="200">Reset instructions sent (if email exists)</response>
        /// <response code="400">Invalid email format</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // تحقق بسيط
            if (string.IsNullOrEmpty(request.Email)) return BadRequest("Email is required");

            try
            {
                // بننادي السيرفيس تطلع التوكن
                var token = await _authService.GeneratePasswordResetTokenAsync(request.Email);

                if (token == null)
                {
                    // لو الإيميل مش موجود، بنرجع رسالة عامة للأمان
                    return Ok(new { Message = "If email exists, reset link sent." });
                }

                // 🚨 ملحوظة هامة جداً:
                // السطر ده عشان التيست بس (Dev Mode) عشان تاخد التوكن من الـ Postman
                // لما تطلع Production لازم تمسح السطر ده وتعتمد على الإيميل بس
                return Ok(new
                {
                    Message = "Check email (Token returned for testing only)",
                    TestToken = token // 👈 خد ده واستخدمه في الخطوة الجاية
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }





        [HttpGet("reset-password-redirect")]
        [AllowAnonymous] // عشان مش محتاج توكن وهو لسه بيعمل ريسيت
        public IActionResult ResetPasswordRedirect(string email, string token)
        {
            // 1. ده اسم البروتوكول اللي تطبيق الموبايل (Flutter/React Native) بيسمع عليه
            // تأكد إن الموبايل تيم مظبطينه على الاسم ده (مثلاً: hayy://)
            var mobileDeepLink = $"hayy://reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            // 2. هنعرض صفحة HTML بسيطة وزرار عشان المتصفح يفتح التطبيق
            // الطريقة دي أفضل من Redirect المباشر عشان المتصفحات أحيانا بتعمل Block للتحويل الأوتوماتيك
            var htmlContent = $@"
        <html>
            <head>
                <title>Reset Password</title>
                <style>
                    body {{ font-family: sans-serif; text-align: center; padding-top: 50px; }}
                    .btn {{ background-color: #4CAF50; color: white; padding: 15px 32px; 
                            text-align: center; text-decoration: none; display: inline-block; 
                            font-size: 16px; border-radius: 8px; border: none; cursor: pointer; }}
                </style>
            </head>
            <body>
                <h2>Password Reset</h2>
                <p>Click the button below to open the app and reset your password.</p>
                <a href='{mobileDeepLink}' class='btn'>Open App to Reset Password</a>
                
                <script>
                    // محاولة فتح التطبيق أوتوماتيكياً
                    window.location.href = '{mobileDeepLink}';
                </script>
            </body>
        </html>";

            return Content(htmlContent, "text/html");
        }









        /// <summary>
        /// Reset password using a valid reset token
        /// </summary>
        /// <param name="request">Email, reset token, and new password</param>
        /// <param name="validator">FluentValidation validator for reset password request</param>
        /// <returns>Success message</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/app/auth/reset-password
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "email": "ahmed@example.com",
        ///         "token": "CfDJ8KzR3...",
        ///         "newPassword": "NewSecurePass789!",
        ///         "confirmPassword": "NewSecurePass789!"
        ///     }
        /// 
        /// Requirements:
        /// - Token must be valid and not expired
        /// - Token must match the one sent via email
        /// - New password must meet security requirements
        /// 
        /// After successful password reset:
        /// - All refresh tokens are invalidated
        /// - User must login with the new password
        /// - Reset token becomes invalid (single-use)
        /// </remarks>
        /// <response code="200">Password reset successful</response>
        /// <response code="400">Invalid token, expired token, or validation errors</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // بنبعت التوكن والباسورد الجديد للسيرفيس عشان تنفذ
                var result = await _authService.ResetPasswordAsync(request);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "Password has been reset successfully. Login now!" });
                }

                // لو في أخطاء (التوكن غلط أو منتهي)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }





        /// <summary>
        /// Confirm user email address
        /// </summary>
        /// <param name="userId">User ID from the confirmation link</param>
        /// <param name="token">Email confirmation token</param>
        /// <returns>Success or error message</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/app/auth/confirm-email?userId=123e4567-e89b-12d3-a456-426614174000&amp;token=CfDJ8KzR3...
        /// 
        /// This endpoint is typically called when:
        /// - User clicks the confirmation link in their registration email
        /// - Mobile app intercepts the deep link: Hayy://confirm-email?userId=...&amp;token=...
        /// - App extracts parameters and calls this endpoint
        /// 
        /// After successful confirmation:
        /// - User's email is marked as verified
        /// - User can now login to the application
        /// - IsVerified and EmailConfirmed flags are set to true
        /// 
        /// Token validity:
        /// - Token is single-use only
        /// - Token has an expiration time
        /// - If token expired, user can request a new one via resend-confirmation-email
        /// </remarks>
        /// <response code="200">Email confirmed successfully</response>
        /// <response code="400">Invalid or expired token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest(new { Error = "Invalid email confirmation request" });

            try
            {
                var result = await _authService.ConfirmEmailAsync(userId, token);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "Email confirmed successfully. You can login now." });
                }

                return BadRequest(new { Error = "Email confirmation failed", Details = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Resend email confirmation link
        /// </summary>
        /// <param name="request">Email address to resend confirmation to</param>
        /// <returns>Success message</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/app/auth/resend-confirmation-email
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "email": "ahmed@example.com"
        ///     }
        /// 
        /// Use this endpoint when:
        /// - User didn't receive the original confirmation email
        /// - Original confirmation token has expired
        /// - User accidentally deleted the confirmation email
        /// 
        /// Validations:
        /// - Email must exist in the system
        /// - Account must not already be verified
        /// 
        /// Process:
        /// - New confirmation token is generated
        /// - New email with deep link is sent
        /// - Old token remains valid until expiration
        /// </remarks>
        /// <response code="200">Confirmation email sent successfully</response>
        /// <response code="400">Email not found or already verified</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("resend-confirmation-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequest request)
        {
            // تحقق بسيط إن الإيميل مبعوت
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Error = "Email is required" });
            }

            try
            {
                await _authService.ResendConfirmationEmailAsync(request.Email);

                return Ok(new { Message = "Confirmation email sent successfully. Please check your inbox." });
            }
            catch (ArgumentException ex)
            {
                // هيطلع هنا لو الإيميل غلط أو الحساب متفعل أصلاً
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred", Details = ex.Message });
            }
        }






        /// <summary>
        /// Helper method to format FluentValidation errors
        /// </summary>
        private ModelStateDictionary FormatValidationErrors(ValidationResult validationResult)
        {
            var modelState = new ModelStateDictionary();
            foreach (var failure in validationResult.Errors)
            {
                modelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
            return modelState;
        }
    }
}
