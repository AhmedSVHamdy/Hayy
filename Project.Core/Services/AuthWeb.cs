using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        public AuthWeb(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IImageService imageService,
            IValidator<RegisterDTO> registerDtoValidator,
            IEmailService emailService,
            IBusinessRepository businessRepo,
            IJwtService jwtService,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _registerDtoValidator = registerDtoValidator;
            _emailService = emailService;
            _businessRepo = businessRepo;
            _jwtService = jwtService;
            _cache = cache;
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
        //  3. LOGIN
        // =========================================================
        public async Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);
            if (user == null) throw new ArgumentException("Invalid Credentials");

            if (!user.IsVerified)
                throw new ArgumentException("Email is not verified. Please check your inbox.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!, false);
            if (!result.Succeeded) throw new ArgumentException("Invalid Credentials");

            var jwtResult = await _jwtService.CreateJwtTokenAsync(user, "web");

            user.RefreshToken = jwtResult.RefreshToken;
            user.RefreshTokenExpirationDateTime = jwtResult.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            string currentStatus = await GetUserVerificationStatus(user);
            bool hasActiveSubscription = await HasActiveSubscriptionAsync(user);

            return new AuthenticationResponse
            {
                PersonName = user.FullName,
                Email = user.Email,
                Token = jwtResult.Token,
                Expiration = jwtResult.Expiration,
                RefreshToken = jwtResult.RefreshToken,
                RefreshTokenExpirationDateTime = jwtResult.RefreshTokenExpirationDateTime,
                UserType = user.UserType,
                VerificationStatus = currentStatus,
                HasActiveSubscription = hasActiveSubscription,
                Id = user.Id
            };
        }

        // =========================================================
        //  4. REFRESH TOKEN
        // =========================================================
        public async Task<AuthenticationResponse> RefreshTokenAsync(TokenDTO tokenDTO)
        {
            if (tokenDTO == null) throw new ArgumentException("Invalid client request");

            var principal = await _jwtService.GetPrincipalFromJwtToken(tokenDTO.AccessToken);
            if (principal == null) throw new ArgumentException("Invalid access token");

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(email!);

            if (user == null || user.RefreshToken != tokenDTO.RefreshToken || user.RefreshTokenExpirationDateTime <= DateTime.UtcNow)
                throw new ArgumentException("Invalid refresh token");

            var newJwtResult = await _jwtService.CreateJwtTokenAsync(user, "web");

            user.RefreshToken = newJwtResult.RefreshToken;
            user.RefreshTokenExpirationDateTime = newJwtResult.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            string currentStatus = await GetUserVerificationStatus(user);
            bool hasActiveSubscription = await HasActiveSubscriptionAsync(user);

            return new AuthenticationResponse
            {
                PersonName = user.FullName,
                Email = user.Email,
                Token = newJwtResult.Token,
                Expiration = newJwtResult.Expiration,
                RefreshToken = newJwtResult.RefreshToken,
                RefreshTokenExpirationDateTime = newJwtResult.RefreshTokenExpirationDateTime,
                UserType = user.UserType,
                VerificationStatus = currentStatus,
                HasActiveSubscription = hasActiveSubscription
            };
        }

        // =========================================================
        //  5. CONFIRM EMAIL
        // =========================================================
        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var isValidOtp = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                token);

            if (!isValidOtp)
                return IdentityResult.Failed(new IdentityError { Description = "Invalid or expired OTP." });

            user.IsVerified = true;
            user.EmailConfirmed = true;
            return await _userManager.UpdateAsync(user);
        }

        // =========================================================
        //  HELPERS
        // =========================================================

        private async Task<string> GetUserVerificationStatus(User user)
        {
            if (user.UserType == UserType.Business.ToString())
            {
                var business = await _businessRepo.GetBusinessByUserIdAsync(user.Id);
                if (business != null)
                    return business.VerificationStatus.ToString();
                return "New";
            }
            else if (user.UserType == UserType.Admin.ToString())
            {
                return VerificationStatus.Verified.ToString();
            }
            return "New";
        }

        // ✅ الجديد: بتشيك لو البيزنس عنده اشتراك نشط
        private async Task<bool> HasActiveSubscriptionAsync(User user)
        {
            // الـ Admin مش محتاج اشتراك
            if (user.UserType != UserType.Business.ToString())
                return true;

            var business = await _businessRepo.GetBusinessByUserIdAsync(user.Id);
            if (business == null) return false;

            return business.Subscriptions
                .Any(s => s.IsActive && s.EndDate > DateTime.UtcNow);
        }

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
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });

            await _userManager.AddToRoleAsync(user, roleName);

            return user;
        }

        private async Task SendConfirmationEmailInternal(User user)
        {
            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
            await _emailService.SendEmailAsync(user.Email!, "Verify Your Email", $"Your OTP is: {otp}");
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found.");
            if (user.IsVerified) throw new ArgumentException("Email already verified.");
            await SendConfirmationEmailInternal(user);
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.MinValue;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            return await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        }

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;
            EnforceOtpRateLimit(email, "password-reset");
            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
            await _emailService.SendEmailAsync(email, "Reset Your Password", $"Your OTP is: {otp}");
            return otp;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            var isValidOtp = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, request.Token);
            if (!isValidOtp) return IdentityResult.Failed(new IdentityError { Description = "Invalid or expired OTP." });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (result.Succeeded)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        public async Task<bool> MakeAdmin(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found with this email.");
            if (await _userManager.IsInRoleAsync(user, "Admin")) throw new ArgumentException("User is already an Admin.");
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            return result.Succeeded;
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            return new UserProfileDTO
            {
                Id = user.Id.ToString(),
                FullName = user.FullName!,
                Email = user.Email,
                ImageUrl = user.ProfileImage,
                UserType = user.UserType!.ToString(),
                IsEmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshToken == token);
            if (user == null) return false;
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.UtcNow.AddDays(-1);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        private const int MaxOtpRequestsPerHour = 5;
        private static readonly TimeSpan OtpWindow = TimeSpan.FromHours(1);

        private sealed class OtpRateState
        {
            public int Count { get; set; }
            public DateTime WindowStartUtc { get; set; }
        }

        private void EnforceOtpRateLimit(string email, string purpose)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var cacheKey = $"otp-rate:{purpose}:{normalizedEmail}";
            var now = DateTime.UtcNow;

            var state = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = OtpWindow;
                return new OtpRateState { Count = 0, WindowStartUtc = now };
            })!;

            if (now - state.WindowStartUtc >= OtpWindow)
            {
                state.Count = 0;
                state.WindowStartUtc = now;
            }

            if (state.Count >= MaxOtpRequestsPerHour)
                throw new InvalidOperationException("OTP limit reached. Please try again after 1 hour.");

            state.Count++;
            _cache.Set(cacheKey, state, OtpWindow);
        }
    }
}