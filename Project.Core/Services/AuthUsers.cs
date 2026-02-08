using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System.Security.Claims;
using System.Text;

namespace Project.Core.Services
{
    public class AuthUsers : IAuthUsers
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly IValidator<RegisterDTO> _registerDtoValidator;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor; // 👈 مهم لجلب عنوان السيرفر

        public AuthUsers(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IImageService imageService,
            IValidator<RegisterDTO> registerDtoValidator,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _registerDtoValidator = registerDtoValidator;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ==========================================================
        // 1. REGISTER
        // ==========================================================
        public async Task<User> Register(RegisterDTO registerDTO, IFormFile? image)
        {
            // 1. Validation
            var valResult = await _registerDtoValidator.ValidateAsync(registerDTO);
            if (!valResult.IsValid)
            {
                var errors = string.Join(", ", valResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }

            // 2. Upload Image
            string? profileImageUrl = null;
            if (image != null && image.Length > 0)
            {
                profileImageUrl = await _imageService.UploadImageAsync(image);
            }

            // 3. Map DTO
            User user = new User()
            {
                FullName = registerDTO.FullName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                ProfileImage = profileImageUrl,
                City = registerDTO.City,
                CreatedAt = DateTime.UtcNow,
                UserType = UserType.User.ToString(),
                IsVerified = false,
                EmailConfirmed = false,
                UserSettings = new UserSettings { }
            };

            // 4. Create User
            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password!);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"Registration Failed: {errors}");
            }

            // 5. Assign Role
            string roleName = UserType.User.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });
            }
            await _userManager.AddToRoleAsync(user, roleName);

            // 6. Send Email
            await SendConfirmationEmailHelper(user);

            return user;
        }

        // ==========================================================
        // 2. LOGIN
        // ==========================================================
        public async Task<User> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null) throw new ArgumentException("Invalid email or password");

            if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password))
                throw new ArgumentException("Invalid email or password");

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
                throw new ArgumentException("Email is not verified. Please check your inbox.");

            return user;
        }

        // ==========================================================
        // 3. CONFIRM EMAIL
        // ==========================================================
        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.IsVerified = true; // تحديث الفلاج الإضافي الخاص بك
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // ==========================================================
        // 4. RESEND CONFIRMATION
        // ==========================================================
        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("Invalid Email");
            if (await _userManager.IsEmailConfirmedAsync(user)) throw new ArgumentException("Email is already confirmed");

            await SendConfirmationEmailHelper(user);
        }

        // ==========================================================
        // 5. FORGOT PASSWORD (GENERATE TOKEN)
        // ==========================================================
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null; // Return null effectively hides user existence (Security Best Practice)

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Prepare Link
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(user.Email!);
            var baseUrl = GetBaseUrl(); // 👈 دالة مساعدة تجيب الرابط أوتوماتيك

            // 🔗 الرابط يوجه للـ API Redirect Endpoint وليس الموبايل مباشرة
            var resetLink = $"{baseUrl}/api/app/auth/reset-password-redirect?email={encodedEmail}&token={encodedToken}";

            var message = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #333;'>Reset Your Password</h2>
                    <p>You requested to reset your password for Hayy App.</p>
                    <p>Click the button below to proceed:</p>
                    <a href='{resetLink}' 
                       style='background-color: #d9534f; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                       Reset Password
                    </a>
                    <p style='margin-top: 20px; font-size: 12px; color: #777;'>If you did not request this, please ignore this email.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, "Reset Your Password", message);
            return "Email sent";
        }

        // ==========================================================
        // 6. RESET PASSWORD (EXECUTE)
        // ==========================================================
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Invalid request" });

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                // 🔒 Security: Revoke Refresh Token to force re-login on all devices
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // ==========================================================
        // 7. CHANGE PASSWORD
        // ==========================================================
        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // 🔒 Security: Revoke Refresh Token
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // ==========================================================
        // 8. LOGOUT
        // ==========================================================
        public async Task<bool> Logout(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.MinValue;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // ==========================================================
        // HELPERS
        // ==========================================================
        private async Task SendConfirmationEmailHelper(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(token);
            var encodedUserId = Uri.EscapeDataString(user.Id.ToString());
            var baseUrl = GetBaseUrl();

            // 🔗 الرابط يوجه للـ API Redirect Endpoint
            var confirmLink = $"{baseUrl}/api/app/auth/confirm-email-redirect?userId={encodedUserId}&token={encodedToken}";

            var message = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #4CAF50;'>Welcome to Hayy App!</h2>
                    <p>Thanks for signing up. Please verify your email to get started.</p>
                    <a href='{confirmLink}' 
                       style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                       Verify Email
                    </a>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, "Verify your email", message);
        }

        // دالة لجلب الدومين الحالي ديناميكياً
        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return "https://localhost:7248"; // Fallback for dev

            // e.g., https://api.hayy.com
            return $"{request.Scheme}://{request.Host}";
        }
    }
}