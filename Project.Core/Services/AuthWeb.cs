using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System.Security.Claims;

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
        //  1. REGISTER BUSINESS
        // =========================================================
        public async Task<RegisterResponse> RegisterBusinessAsync(RegisterDTO registerDTO, IFormFile? image)
        {
            var user = await RegisterUserInternal(registerDTO, image, UserType.Business, isVerified: false);
            await SendConfirmationEmailInternal(user);

            return new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email!,
                Message = "Registration successful. Please check your email to verify your account.",
                RequiresEmailConfirmation = true
            };
        }

        // =========================================================
        //  2. REGISTER ADMIN
        // =========================================================
        public async Task<RegisterResponse> RegisterAdminAsync(RegisterDTO registerDTO, IFormFile? image)
        {
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
        //  3. LOGIN (Updated with Refresh Token Logic ✅)
        // =========================================================
        public async Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);
            if (user == null) throw new ArgumentException("Invalid Credentials");

            if (!user.IsVerified)
            {
                throw new ArgumentException("Email is not verified. Please check your inbox.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!, false);
            if (!result.Succeeded) throw new ArgumentException("Invalid Credentials");

            // 1. توليد التوكن
            var jwtResult = await _jwtService.CreateJwtTokenAsync(user, "web");

            // 2. 🔥 حفظ الـ Refresh Token في قاعدة البيانات 🔥
            user.RefreshToken = jwtResult.RefreshToken;
            user.RefreshTokenExpirationDateTime = jwtResult.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            // 3. تحديد الحالة
            string currentStatus = await GetUserVerificationStatus(user);

            return new AuthenticationResponse
            {
                PersonName = user.FullName,
                Email = user.Email,
                Token = jwtResult.Token,
                Expiration = jwtResult.Expiration,
                RefreshToken = jwtResult.RefreshToken,
                RefreshTokenExpirationDateTime = jwtResult.RefreshTokenExpirationDateTime,
                UserType = user.UserType,
                VerificationStatus = currentStatus
            };
        }

        // =========================================================
        //  🔥 4. REFRESH TOKEN (New Method) 🔥
        // =========================================================
        public async Task<AuthenticationResponse> RefreshTokenAsync(TokenDTO tokenDTO)
        {
            if (tokenDTO == null) throw new ArgumentException("Invalid client request");

            // 1. استخراج بيانات المستخدم من التوكن المنتهي
            var principal = await _jwtService.GetPrincipalFromJwtToken(tokenDTO.AccessToken);
            if (principal == null) throw new ArgumentException("Invalid access token");

            // البحث عن الإيميل داخل الـ Claims
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(email!);

            // 2. التحقق من صلاحية الـ Refresh Token
            if (user == null || user.RefreshToken != tokenDTO.RefreshToken || user.RefreshTokenExpirationDateTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Invalid refresh token");
            }

            // 3. توليد توكن جديد
            var newJwtResult = await _jwtService.CreateJwtTokenAsync(user, "web");

            // 4. تحديث الداتابيز بالتوكن الجديد (Rotation)
            user.RefreshToken = newJwtResult.RefreshToken;
            user.RefreshTokenExpirationDateTime = newJwtResult.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            // 5. إرجاع النتيجة
            string currentStatus = await GetUserVerificationStatus(user);

            return new AuthenticationResponse
            {
                PersonName = user.FullName,
                Email = user.Email,
                Token = newJwtResult.Token,
                Expiration = newJwtResult.Expiration,
                RefreshToken = newJwtResult.RefreshToken,
                RefreshTokenExpirationDateTime = newJwtResult.RefreshTokenExpirationDateTime,
                UserType = user.UserType,
                VerificationStatus = currentStatus
            };
        }

        // =========================================================
        //  5. CONFIRM EMAIL
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
        //  6. RESEND CONFIRMATION
        // =========================================================
        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found");
            if (user.IsVerified) throw new ArgumentException("Email is already verified.");

            await SendConfirmationEmailInternal(user);
        }

        // =========================================================
        //  7. LOGOUT
        // =========================================================
        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // مسح الـ Refresh Token عند الخروج
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.MinValue;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // =========================================================
        //  8. CHANGE PASSWORD
        // =========================================================
        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // تصفير التوكن لضمان الخروج من الأجهزة الأخرى
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // =========================================================
        //  9. FORGOT PASSWORD
        // =========================================================
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);

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
        //  10. RESET PASSWORD
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
        //  11. MAKE ADMIN
        // =========================================================
        public async Task<bool> MakeAdmin(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found with this email.");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                throw new ArgumentException("User is already an Admin.");
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            return result.Succeeded;
        }

        // =========================================================
        //  HELPERS
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

            var confirmLink = $"https://localhost:7248/api/web/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";

            var message = $@"<h3>Welcome!</h3>
                             <p>Please confirm your account:</p>
                             <a href='{confirmLink}'>Confirm Account</a>";

            await _emailService.SendEmailAsync(user.Email!, "Confirm Account", message);
        }

        // دالة مساعدة لتحديد حالة المستخدم (عشان نستخدمها في Login و RefreshToken)
        private async Task<string> GetUserVerificationStatus(User user)
        {
            if (user.UserType == UserType.Business.ToString())
            {
                var business = await _businessRepo.GetBusinessByUserIdAsync(user.Id);
                if (business != null)
                {
                    return business.VerificationStatus.ToString();
                }
                return "New";
            }
            else if (user.UserType == UserType.Admin.ToString())
            {
                return VerificationStatus.Verified.ToString();
            }
            return "New";
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
        {
            // 1. هات اليوزر من الداتابيز
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // 2. حول البيانات لـ DTO (Mapping)
            // لاحظ: لو عندك خصائص تانية في الـ ApplicationUser ضيفها هنا
            return new UserProfileDTO
            {
                Id = user.Id.ToString(),
               FullName= user.FullName!,
                Email = user.Email,
                ImageUrl = user.ProfileImage, // لو بتخزن مسار الصورة
                UserType = user.UserType!.ToString(), // لو بتستخدم Enum
                IsEmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            // بنبحث عن اليوزر بالإيميل
            var user = await _userManager.FindByEmailAsync(email);

            // لو النتيجة مش null يبقى الإيميل موجود، لو null يبقى متاح
            return user != null;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            // 1. بندور على اليوزر اللي معاه الـ Refresh Token ده
            // ملحوظة: لازم يكون عندك access للـ UserManager أو الـ DBContext مباشرة
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshToken == token);

            if (user == null) return false;

            // 2. بنلغي التوكن (يا إما بخليه null أو بخليه منتهي الصلاحية)
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.UtcNow.AddDays(-1); // تاريخ قديم عشان يموت فوراً

            // 3. نحفظ التغييرات
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}