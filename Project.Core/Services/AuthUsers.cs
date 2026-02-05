using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;

namespace Project.Core.Services
{
    public class AuthUsers : IAuthUsers
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IImageService _imageService;   // التعامل مع صور Azure
        private readonly IValidator<RegisterDTO> _registerDtoValidator;
        private readonly IEmailService _emailService;



        public AuthUsers(UserManager<User> userManager,
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
        public async Task<User> Register(RegisterDTO registerDTO, IFormFile? image)

        {
            //  Validate DTO
            var valResult = await _registerDtoValidator.ValidateAsync(registerDTO);
            if (!valResult.IsValid)
            {
                // ValidationException
                var errors = string.Join(", ", valResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }

            //  Upload Image
            string? profileImageUrl = null;
            if (image != null && image.Length > 0)
            {
                profileImageUrl = await _imageService.UploadImageAsync(image);
            }

            // Map DTO to Entity
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

            // Add User
            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password!);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"Registration Failed: {errors}");
            }

            //  Handle Roles
            string roleName = UserType.User.ToString();

            // check role
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                ApplicationRole applicationRole = new ApplicationRole()
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                };
                // create role
                await _roleManager.CreateAsync(applicationRole);
            }
            // add role
            await _userManager.AddToRoleAsync(user, roleName);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(token);
            var encodedUserId = Uri.EscapeDataString(user.Id.ToString());


            // ⚠️ هام: ده رابط السيرفر بتاعك (لو شغال لوكال حط اللوكال هوست، لو مرفوع حط الدومين)
            // مثال لوكال: https://localhost:7001
            // مثال مرفوع: https://api.hayy-app.com
            string baseUrl = "https://localhost:7248";

            // التعديل هنا: اللينك بقى HTTP عادي جداً
            var confirmLink = $"{baseUrl}/api/app/auth/confirm-email-redirect?userId={encodedUserId}&token={encodedToken}";
            var message = $@"
    <div style='font-family: Arial, sans-serif; padding: 20px;'>
        <h3>Welcome to Hayy App!</h3>
        <p>Please confirm your account by clicking the button below:</p>
        <a href='{confirmLink}' 
           style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>
           Confirm My Account
        </a>
        <p>If the button doesn't work, copy this link to your browser:</p>
        <p>{confirmLink}</p>
    </div>";

            await _emailService.SendEmailAsync(user.Email!, "Confirm your email", message);

            return user;
        }





        public async Task<User> Login(LoginDTO loginDTO)
        {
            //  Check if user exists
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);

            if (user == null)
            {
                throw new ArgumentException("Invalid Email or Password");
            }

            if (!user.IsVerified) // أو !user.EmailConfirmed
            {
                throw new ArgumentException("Email is not verified. Please check your inbox.");
            }

            //  Check Password
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                throw new ArgumentException("Invalid Email or Password");
            }


            return user;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            // Identity بتقوم بالواجب وتتأكد من التوكن
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                // نحدث الحقل الخاص بيك كمان
                user.IsVerified = true;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // نرجع خطأ مخصص لو المستخدم مش موجود
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            // 1. تغيير الباسورد
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // 2. ⚠️ خطوة أمنية مهمة: تصفير الـ Refresh Token
                // عشان نضمن خروج أي جهاز تاني داخل بنفس الحساب
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;

                // 3. تحديث SecurityStamp (اختياري ولكنه مفيد لإلغاء التوكنات القديمة لو بتستخدم Cookies)
                // await _userManager.UpdateSecurityStampAsync(user);

                await _userManager.UpdateAsync(user);
            }

            return result;
        }


        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // تشفير البيانات عشان اللينك
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);

            // لينك الموبايل (Deep Link)
            var resetLink = $"Hayy://reset-password?email={encodedEmail}&token={encodedToken}";

            // تجهيز الرسالة HTML
            var message = $@"
            <h3>Password Reset Request</h3>
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <br/>
            <p>If you didn't request this, please ignore this email.</p>";

            // إرسال الإيميل بجد ✅
            await _emailService.SendEmailAsync(email, "Reset Password", message);

            return "Email sent"; // مش هنرجع اللينك خلاص لأسباب أمنية
        }



        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                // بنرجع خطأ عام عشان منوضحش للهاكر إن الإيميل مش موجود
                return IdentityResult.Failed(new IdentityError { Description = "Invalid request" });
            }

            // 1. محاولة تغيير الباسورد باستخدام التوكن
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                // 2. ⚠️ خطوة أمنية: تصفير الـ Refresh Token لإجبار تسجيل الدخول من جديد
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            // 1. ندور على اليوزر
            var user = await _userManager.FindByEmailAsync(email);

            // لو مش موجود، نوقف (ممكن ترمي خطأ أو تتجاهل لأسباب أمنية)
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // 2. لو هو أصلاً مفعل، ملوش لازمة نبعت تاني
            if (user.IsVerified)
            {
                throw new ArgumentException("Email is already verified. You can login directly.");
            }

            // 3. نكون التوكن من جديد (عشان لو القديم انتهت صلاحيته)
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // 4. نجهز اللينك (نفس الكود اللي في Register)
            var encodedToken = Uri.EscapeDataString(token);
            var encodedUserId = Uri.EscapeDataString(user.Id.ToString());

            var confirmLink = $"Hayy://confirm-email?userId={encodedUserId}&token={encodedToken}";

            var message = $@"
        <h3>Resend Confirmation</h3>
        <p>You requested to resend the confirmation email.</p>
        <p>Please confirm your account by clicking the link below:</p>
        <a href='{confirmLink}'>Confirm My Account</a>";

            // 5. إرسال الإيميل
            await _emailService.SendEmailAsync(user.Email!, "Resend Confirmation Email", message);
        }


        public async Task<bool> Logout(string userId)
        {
            // 1. البحث عن المستخدم
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false; // المستخدم مش موجود

            // 2. تصفير الـ Refresh Token (أهم خطوة)
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.MinValue; // أو خليها وقت في الماضي

            // 3. حفظ التغييرات
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}

