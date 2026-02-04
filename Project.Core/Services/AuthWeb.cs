using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System.Text;

namespace Project.Core.Services
{
    public class AuthWeb : IAuthWeb
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly IValidator<RegisterDTO> _registerDtoValidator;
        private readonly IEmailService _emailService; // 👈 إضافة خدمة الإيميل

        public AuthWeb(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IImageService imageService,
            IValidator<RegisterDTO> registerDtoValidator,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _registerDtoValidator = registerDtoValidator;
            _emailService = emailService;
        }

        // =========================================================
        //  1. REGISTER BUSINESS (With Email Logic)
        // =========================================================
        public async Task<User> RegisterBusinessAsync(RegisterDTO registerDTO, IFormFile? image)
        {
            // 1. إنشاء المستخدم (غير مفعل)
            var user = await RegisterUserInternal(registerDTO, image, UserType.Business, isVerified: false);

            // 2. إرسال إيميل التفعيل
            await SendConfirmationEmailInternal(user);

            return user;
        }

        // =========================================================
        //  2. REGISTER ADMIN (Auto Verified)
        // =========================================================
        public async Task<User> RegisterAdminAsync(RegisterDTO registerDTO, IFormFile? image)
        {
            // الأدمن بيتعمل مفعل جاهز لأن أدمن تاني هو اللي عمله
            return await RegisterUserInternal(registerDTO, image, UserType.Admin, isVerified: true);
        }

        // =========================================================
        //  3. LOGIN (With Verification Check)
        // =========================================================
        public async Task<User> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);
            if (user == null) throw new ArgumentException("Invalid Credentials");

            // ✅ التحقق من التفعيل (مهم للبزنس)
            if (!user.IsVerified)
            {
                throw new ArgumentException("Email is not verified. Please check your inbox.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!, false);
            if (!result.Succeeded) throw new ArgumentException("Invalid Credentials");

            return user;
        }

        // =========================================================
        //  4. CONFIRM EMAIL
        // =========================================================
        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.IsVerified = true;
                await _userManager.UpdateAsync(user);
            }
            return result;
        }

        // =========================================================
        //  5. RESEND CONFIRMATION
        // =========================================================
        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found");
            if (user.IsVerified) throw new ArgumentException("Email is already verified.");

            await SendConfirmationEmailInternal(user);
        }

        // =========================================================
        //  HELPERS (DRY Principle)
        // =========================================================
        private async Task<User> RegisterUserInternal(RegisterDTO dto, IFormFile? image, UserType userType, bool isVerified)
        {
            var valResult = await _registerDtoValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ArgumentException(string.Join(", ", valResult.Errors.Select(e => e.ErrorMessage)));

            string? profileImageUrl = image != null ? await _imageService.UploadImageAsync(image) : null;

            User user = new User()
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                ProfileImage = profileImageUrl,
                City = dto.City,
                CreatedAt = DateTime.UtcNow,
                UserType = userType.ToString(),
                IsVerified = isVerified,
                EmailConfirmed = isVerified,
                UserSettings = new UserSettings { }
            };

            var result = await _userManager.CreateAsync(user, dto.Password!);
            if (!result.Succeeded) throw new ArgumentException($"Failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            string roleName = userType.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });
            }
            await _userManager.AddToRoleAsync(user, roleName);

            return user;
        }

        private async Task SendConfirmationEmailInternal(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var encodedUserId = Uri.EscapeDataString(user.Id.ToString());

            // ⚠️ ملحوظة: ده لينك الويب فرونت إند، مش الموبايل
            // لو معندكش فرونت إند وعاوز تضرب الـ API علطول:
            // var url = $"https://api.mydomain.com/api/web/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";

            // بس الأصح يروح لصفحة React/Angular:
            var confirmLink = $"https://my-dashboard-website.com/verify?userId={encodedUserId}&token={encodedToken}";

            var message = $@"<h3>Welcome Business Partner!</h3>
                             <p>Please confirm your dashboard account:</p>
                             <a href='{confirmLink}'>Confirm Account</a>";

            await _emailService.SendEmailAsync(user.Email!, "Confirm Dashboard Account", message);
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // مسح الـ Refresh Token لمنع تجديد الجلسة
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.MinValue;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // =========================================================
        //  7. CHANGE PASSWORD
        // =========================================================
        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // أمان: تسجيل خروج من كل الأجهزة
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // =========================================================
        //  8. FORGOT PASSWORD (Generate Token)
        // =========================================================
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null; // User not found

            // 1. Generate Token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 2. Encode
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);

            // 3. Create Web Link (Difference is here ⚠️)
            // لينك يودي لصفحة الويب Frontend
            var resetLink = $"https://my-dashboard.com/reset-password?email={encodedEmail}&token={encodedToken}";

            var message = $@"
            <h3>Password Reset Request</h3>
            <p>Click the link below to reset your password for the Web Portal:</p>
            <a href='{resetLink}'>Reset Password</a>";

            // 4. Send Email
            await _emailService.SendEmailAsync(email, "Reset Web Password", message);

            return "Email sent";
        }

        // =========================================================
        //  9. RESET PASSWORD (Consume Token)
        // =========================================================
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Invalid request" });

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                // أمان: مسح الـ Refresh Token
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }
    }
}