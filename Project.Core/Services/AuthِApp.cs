using FluentValidation;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration; // 👈 مهم
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System.Security.Claims;

namespace Project.Core.Services
{
    public class AuthApp : IAuthUsers
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly IValidator<RegisterDTO> _registerDtoValidator;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthApp(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IImageService imageService,
            IValidator<RegisterDTO> registerDtoValidator,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _registerDtoValidator = registerDtoValidator;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        // ==========================================================
        // 1. REGISTER
        // ==========================================================
        public async Task<RegisterResponse> Register(RegisterDTO registerDTO, IFormFile? image)
        {
            // 1. Validation
            var valResult = await _registerDtoValidator.ValidateAsync(registerDTO);
            if (!valResult.IsValid)
            {
                var errors = string.Join(", ", valResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }

            // 2. Upload Image (Optional)
            string? profileImageUrl = null;
            if (image != null && image.Length > 0)
            {
                profileImageUrl = await _imageService.UploadImageAsync(image);
            }

            // 3. Map DTO to Entity
            User user = new User()
            {
                FullName = registerDTO.FullName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email, // Identity requires UserName
                ProfileImage = profileImageUrl,
                City = registerDTO.City,
                CreatedAt = DateTime.UtcNow,
                UserType = UserType.User.ToString(), // Default to User
                IsVerified = false,
                EmailConfirmed = false
            };

            // 4. Create User in DB
            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password!);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"Registration Failed: {errors}");
            }

            var existingSettings = await _unitOfWork.GetRepository<UserSettings>()
            .GetAsync(s => s.UserId == user.Id);

            if (existingSettings == null)
            {
                UserSettings userSettings = new UserSettings()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EmailNotifications = true,
                    NotificationsEnabled = true
                };

                await _unitOfWork.GetRepository<UserSettings>().AddAsync(userSettings);
            }
            await _unitOfWork.SaveChangesAsync();





            // 5. Assign Role
            string roleName = UserType.User.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });
            }
            await _userManager.AddToRoleAsync(user, roleName);

            // 6. Send Confirmation Email
            await SendConfirmationEmailHelper(user);

            // 7. Return RegisterResponse DTO
            return new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email!,
                Message = "Registration successful. Please check your email to verify your account.",
                RequiresEmailConfirmation = true
            };
        }

        // ==========================================================
        // 2. LOGIN
        // ==========================================================
        public async Task<AuthenticationResponse> Login(LoginDTO loginDTO)
        {
            // 1. Find User
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);
            if (user == null) throw new ArgumentException("Invalid email or password");

            // 2. Check Password
            if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password!))
                throw new ArgumentException("Invalid email or password");

            // 3. Check Email Confirmation
            if (!await _userManager.IsEmailConfirmedAsync(user))
                throw new ArgumentException("Email is not verified. Please check your inbox.");

            // 4. Generate JWT & Refresh Token using IJwtService
            var authResult = await _jwtService.CreateJwtTokenAsync(user, "Mobile");

            // 5. Update User with new Refresh Token (Persistence)
            user.RefreshToken = authResult.RefreshToken;
            user.RefreshTokenExpirationDateTime = authResult.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            // 6. Get User Role & Status
            var roles = await _userManager.GetRolesAsync(user);
            var mainRole = roles.FirstOrDefault() ?? "User";
            string status = user.IsVerified ? "Verified" : "Pending";

            // 7. Return AuthenticationResponse DTO
            return new AuthenticationResponse
            {
                PersonName = user.FullName,
                Email = user.Email,
                Token = authResult.Token,
                Expiration = authResult.Expiration,
                RefreshToken = authResult.RefreshToken,
                RefreshTokenExpirationDateTime = authResult.RefreshTokenExpirationDateTime,
                UserType = mainRole,
                VerificationStatus = status
            };
        }

        // ==========================================================
        // 3. REFRESH TOKEN (Updated ✅)
        // ==========================================================
        public async Task<AuthenticationResponse> RefreshTokenAsync(TokenDTO tokenModel)
        {
            if (tokenModel == null) throw new ArgumentException("Invalid client request");

            // 1. استخراج البيانات من التوكن القديم (تم التعديل لـ AccessToken)
            var principal = await _jwtService.GetPrincipalFromJwtToken(tokenModel.AccessToken);
            if (principal == null) throw new ArgumentException("Invalid access token");

            // 2. البحث عن اليوزر
            string? email = principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email!);

            // 3. التحقق من صحة الـ Refresh Token المحفوظ في الداتابيز
            if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpirationDateTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Invalid or expired refresh token");
            }

            // 4. إنشاء توكن جديد
            // بنستخدم "Mobile" هنا عشان ده سيرفس الأبلكيشن
            var newAuthResponse = await _jwtService.CreateJwtTokenAsync(user, "Mobile");

            // 5. تحديث الداتابيز
            user.RefreshToken = newAuthResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = newAuthResponse.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            // 6. ضبط البيانات الإضافية عشان الفرونت إند ميتفاجئش إنها فاضية
            var roles = await _userManager.GetRolesAsync(user);
            newAuthResponse.UserType = roles.FirstOrDefault() ?? "User";
            newAuthResponse.VerificationStatus = user.IsVerified ? "Verified" : "Pending";

            return newAuthResponse;
        }

        // ==========================================================
        // 4. CONFIRM EMAIL
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
                // Custom Flag Update
                user.IsVerified = true;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // ==========================================================
        // 5. RESEND CONFIRMATION
        // ==========================================================
        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("Invalid Email");
            if (await _userManager.IsEmailConfirmedAsync(user)) throw new ArgumentException("Email is already confirmed");

            await SendConfirmationEmailHelper(user);
        }

        // ==========================================================
        // 6. FORGOT PASSWORD (Generate Token)
        // ==========================================================
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null; // Security: Don't reveal user existence

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Prepare Link Logic
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(user.Email!);
            var baseUrl = GetBaseUrl();

            // Link points to Controller, which redirects to App Scheme
            var resetLink = $"{baseUrl}/api/app/auth/reset-password-redirect?email={encodedEmail}&token={encodedToken}";

            var message = $@"
                <h3>Reset Password</h3>
                <p>Click here to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>";

            await _emailService.SendEmailAsync(user.Email!, "Reset Your Password", message);
            return "Email sent";
        }

        // ==========================================================
        // 7. RESET PASSWORD (Execute)
        // ==========================================================
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Invalid request" });

            if (request.NewPassword != request.ConfirmPassword)
                return IdentityResult.Failed(new IdentityError { Description = "Passwords do not match" });

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                // Security: Revoke Refresh Token
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // ==========================================================
        // 8. CHANGE PASSWORD
        // ==========================================================
        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // Security: Revoke Refresh Token
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        // ==========================================================
        // 9. LOGOUT
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

        public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
        {
            // 1. البحث عن المستخدم باستخدام الـ ID
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // 2. تحويل البيانات (Mapping) من Entity لـ DTO
            // ملحوظة: لو عندك دالة بتعمل Full Path للصورة، استخدمها هنا
            return new UserProfileDTO
            {
                Id = user.Id.ToString(),
                FullName = user.FullName!, // دمج الاسم
                Email = user.Email,
                ImageUrl = user.ProfileImage, // اسم العمود في الداتابيز للصورة
                UserType = user.UserType!.ToString(),
                IsEmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<bool> DeleteAccountAsync(string userId)
        {
            // 1. البحث عن المستخدم
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false; // المستخدم غير موجود أصلاً
            }

            //. (اختياري) حذف الصورة من السيرفر لو موجودة
            
            if (!string.IsNullOrEmpty(user.ProfileImage))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImage);
                if (File.Exists(imagePath)) File.Delete(imagePath);
            }
            

            // 3. حذف المستخدم (سيقوم تلقائياً بحذف الـ Claims والـ Roles المرتبطة به)
            var result = await _userManager.DeleteAsync(user);

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

            var confirmLink = $"{baseUrl}/api/app/auth/confirm-email-redirect?userId={encodedUserId}&token={encodedToken}";

            var message = $@"
                <h3>Welcome!</h3>
                <p>Please confirm your email by clicking below:</p>
                <a href='{confirmLink}'>Verify Email</a>";

            await _emailService.SendEmailAsync(user.Email!, "Verify your email", message);
        }


        public async Task<AuthenticationResponse> GoogleLoginAsync(SocialLoginDTO request, string role = "User")
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                // 1. تجهيز قائمة الـ Client IDs المسموح بها (موبايل + ويب)
                var audiences = new List<string>();

                // ضيف الـ Client ID بتاع الموبايل لو موجود
                if (!string.IsNullOrEmpty(_configuration["Google:ClientId"]))
                    audiences.Add(_configuration["Google:ClientId"]!);

                // ضيف الـ Client ID بتاع الويب لو موجود
                if (!string.IsNullOrEmpty(_configuration["Google:WebClientId"]))
                    audiences.Add(_configuration["Google:WebClientId"]!);

                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = audiences
                };

                // التحقق من التوكن
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (Exception ex)
            {
                // لو التوكن غير صالح أو الـ Audience مش مطابق
                throw new ArgumentException("Invalid Google Token: " + ex.Message);
            }

            // 2. البحث عن المستخدم في قاعدة البيانات بالإيميل
            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // 🆕 حالة مستخدم جديد: إنشاء حساب (Register)
                user = new User
                {
                    // بيانات الهوية الأساسية
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true, // جوجل أكد الإيميل

                    // بيانات الجدول الجديد (User Entity)
                    FullName = payload.Name,       // الاسم
                    ProfileImage = payload.Picture, // الصورة

                    // 👇 هنا السحر: بنحدد النوع بناءً على الباراميتر (User للعميل، Business للشركة)
                    UserType = role,

                    IsVerified = true,             // تفعيل الحساب فوراً
                    CreatedAt = DateTime.UtcNow,   // تاريخ الإنشاء

                    // تهيئة القوائم لتجنب Null Reference لاحقاً
                    Businesses = new List<Business>(),
                    Reviews = new List<Review>()
                };

                // إنشاء باسورد عشوائي قوي جداً (لأنه مش هيستخدمه)
                var randomPassword = "Google_" + Guid.NewGuid().ToString("N") + "_P@ssw0rd";

                var result = await _userManager.CreateAsync(user, randomPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create Google user: {errors}");
                }

                // (اختياري) إضافة Role لو بتستخدم RoleManager
                 await _userManager.AddToRoleAsync(user, role);
            }
            else
            {
                // 🔄 حالة مستخدم موجود: (Login)
                // ممكن نحدث بياناته لو حابب (اختياري)
                
                if (user.ProfileImage != payload.Picture)
                {
                    user.ProfileImage = payload.Picture;
                    await _userManager.UpdateAsync(user);
                }
                
            }

            // 3. إنشاء التوكن (JWT)
            string platform = (role == UserType.Business.ToString()) ? "Web" : "Mobile";

            var authResponse = await _jwtService.CreateJwtTokenAsync(user, platform); // 👈 تم التصحيح

            // التأكد من ملء البيانات الإضافية في الـ Response
            authResponse.UserType = user.UserType;
            authResponse.VerificationStatus = user.IsVerified ? "Verified" : "Pending";

            return authResponse;
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return "https://localhost:7248";
            return $"{request.Scheme}://{request.Host}";
        }

      
    }
}