using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using Project.Core.Services;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/web/auth")]
    [ApiController]
    public class WebAuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly IValidator<RegisterDTO> _registerDtoValidator;
        private readonly IEmailService _emailService;
        private readonly IAuthWeb _authWeb;
        private readonly IJwtService _jwtService;


        public WebAuthController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IImageService imageService,
            IValidator<RegisterDTO> registerDtoValidator,
            IEmailService emailService,
            IAuthWeb authWeb,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _registerDtoValidator = registerDtoValidator;
            _emailService = emailService;
            _authWeb = authWeb;
            _jwtService = jwtService;
        }



        // 1. تسجيل Business (تعديل: إرجاع رسالة فقط بدون توكن)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterBusiness([FromForm] RegisterDTO registerDTO, IFormFile? image)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                User registeredUser = await _authWeb.RegisterBusinessAsync(registerDTO, image);

                // ✅ التعديل: مفيش توكن هنا، لازم يفعل الأول
                return Ok(new
                {
                    Message = "Registration successful. Please check your email to verify your account.",
                    UserId = registeredUser.Id
                });
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

        // 2. تسجيل الدخول (السيرفيس بتتشيك على التفعيل)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                User user = await _authWeb.LoginAsync(loginDTO);

                if (user.UserType != UserType.Business.ToString() && user.UserType != UserType.Admin.ToString())
                    return Unauthorized(new { Error = "Access denied." });

                var authResponse = await _jwtService.CreateJwtTokenAsync(user, "web");
                return Ok(authResponse);
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
        }

        // 3. تفعيل الإيميل (جديد للويب)
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid request");

            try
            {
                var result = await _authWeb.ConfirmEmailAsync(userId, token);

                if (result.Succeeded)
                {
                    // ممكن هنا تعمل Redirect لصفحة Login في الفرونت إند
                    // return Redirect("https://my-dashboard.com/login?verified=true");
                    return Ok(new { Message = "Email confirmed successfully. You can login now." });
                }

                return BadRequest(new { Error = "Email confirmation failed", Details = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 4. إعادة إرسال التفعيل (جديد)
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

        // 5. إنشاء أدمن (زي ما هو)
        [HttpPost("create-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdmin([FromForm] RegisterDTO registerDTO, IFormFile? image)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                User newAdmin = await _authWeb.RegisterAdminAsync(registerDTO, image);
                return Ok(new { Message = "Admin created successfully.", AdminId = newAdmin.Id });
            }
            catch (ArgumentException ex) { return BadRequest(new { Error = ex.Message }); }
        }




        // =========================================================
        //  LOGOUT
        // =========================================================
        [HttpPost("logout")]
        [Authorize] // لازم يكون عامل دخول عشان يعمل خروج
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            bool isLoggedOut = await _authWeb.LogoutAsync(userId);
            if (!isLoggedOut) return BadRequest("Logout failed.");

            return Ok(new { Message = "Logged out successfully" });
        }

        // =========================================================
        //  CHANGE PASSWORD
        // =========================================================
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
        //  FORGOT PASSWORD
        // =========================================================
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request,
            [FromServices] IValidator<ForgotPasswordRequest> validator)
        {
            var valResult = await validator.ValidateAsync(request);
            if (!valResult.IsValid) return BadRequest(valResult.ToDictionary());

            // لن نرجع خطأ إذا كان الإيميل غير موجود لأسباب أمنية
            await _authWeb.GeneratePasswordResetTokenAsync(request.Email);

            return Ok(new { Message = "If the email exists, a reset link has been sent." });
        }

        // =========================================================
        //  RESET PASSWORD
        // =========================================================
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
    
}}