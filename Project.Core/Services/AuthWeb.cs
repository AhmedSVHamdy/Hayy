using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
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
        private readonly IEmailService _emailService;
        private readonly IBusinessRepository _businessRepo;
        private readonly IJwtService _jwtService;

        public AuthWeb(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IImageService imageService,
            IValidator<RegisterDTO> registerDtoValidator,
            IEmailService emailService,
            IBusinessRepository businessRepo,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _registerDtoValidator = registerDtoValidator;
            _emailService = emailService;
            _businessRepo = businessRepo;
            _jwtService = jwtService;
        }

        // =========================================================
        //  1. REGISTER BUSINESS (Updated ✅)
        // =========================================================
        public async Task<RegisterResponse> RegisterBusinessAsync(RegisterDTO registerDTO, IFormFile? image)
        {
            // إنشاء اليوزر داخلياً
            var user = await RegisterUserInternal(registerDTO, image, UserType.Business, isVerified: false);

            // إرسال الإيميل
            await SendConfirmationEmailInternal(user);

            // إرجاع DTO نظيف (بدون توكن)
            return new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email!,
                Message = "Registration successful. Please check your email to verify your account.",
                RequiresEmailConfirmation = true
            };
        }

        // =========================================================
        //  2. REGISTER ADMIN (Updated ✅)
        // =========================================================
        public async Task<RegisterResponse> RegisterAdminAsync(RegisterDTO registerDTO, IFormFile? image)
        {
            // الأدمن بيتفعل علطول (isVerified: true)
            var user = await RegisterUserInternal(registerDTO, image, UserType.Admin, isVerified: true);

            return new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email!,
                Message = "Admin registered successfully.",
                RequiresEmailConfirmation = false
            };
        }

        // =========================================================
        //  3. LOGIN (Returns AuthResponse with Token)
        // =========================================================
        public async Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);
            if (user == null) throw new ArgumentException("Invalid Credentials");

            // التحقق من التفعيل
            if (!user.IsVerified)
            {
                throw new ArgumentException("Email is not verified. Please check your inbox.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!, false);
            if (!result.Succeeded) throw new ArgumentException("Invalid Credentials");

            // توليد التوكن
            var jwtResult = await _jwtService.CreateJwtTokenAsync(user, "web");

            // تحديد حالة البيزنس (Logic التوجيه)
            string currentStatus = "New";

            if (user.UserType == UserType.Business.ToString())
            {
                var business = await _businessRepo.GetByUserIdAsync(user.Id);

                if (business != null)
                {
                    currentStatus = business.VerificationStatus.ToString();
                }
                else
                {
                    currentStatus = "New";
                }
            }
            else if (user.UserType == UserType.Admin.ToString())
            {
                currentStatus = VerificationStatus.Verified.ToString();
            }

            return new AuthenticationResponse
            {
                PersonName = user.FullName,
                Email = user.Email,
                Token = jwtResult.Token,
                Expiration = jwtResult.Expiration,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpirationDateTime = user.RefreshTokenExpirationDateTime,
                UserType = user.UserType,
                VerificationStatus = currentStatus
            };
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
                user.EmailConfirmed = true;
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
        //  6. LOGOUT
        // =========================================================
        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

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
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // =========================================================
        //  8. FORGOT PASSWORD
        // =========================================================
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);

            // رابط الفرونت إند
            var resetLink = $"http://localhost:3000/reset-password?email={encodedEmail}&token={encodedToken}";

            var message = $@"
            <h3>Password Reset Request</h3>
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <br/>
            <p>Or use this token: {token}</p>";

            await _emailService.SendEmailAsync(email, "Reset Password", message);

            return token;
        }

        // =========================================================
        //  9. RESET PASSWORD
        // =========================================================
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Invalid request" });

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // =========================================================
        //  HELPERS
        // =========================================================
        // دالة مساعدة داخلية فقط، بترجع User عشان نقدر نستخدمه في GenerateToken
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

            // تأكد إن الرابط ده مطابق للكنترولر
            var confirmLink = $"https://localhost:7248/api/web/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";

            var message = $@"<h3>Welcome!</h3>
                             <p>Please confirm your account:</p>
                             <a href='{confirmLink}'>Confirm Account</a>";

            await _emailService.SendEmailAsync(user.Email!, "Confirm Account", message);
        }
    }
}