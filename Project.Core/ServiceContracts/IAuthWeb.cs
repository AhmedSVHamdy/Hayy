using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IAuthWeb
    {
        // تسجيل حساب بزنس جديد (مفتوح للعامة)
        Task<User> RegisterBusinessAsync(RegisterDTO registerDTO, IFormFile? image);

        // تسجيل أدمن جديد (خاص بالأدمن فقط)
        Task<User> RegisterAdminAsync(RegisterDTO registerDTO, IFormFile? image);

        // تسجيل الدخول (للأدمن والبزنس)
        Task<User> LoginAsync(LoginDTO loginDTO);


        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);

        // 5. إعادة إرسال التفعيل (جديد)
        Task ResendConfirmationEmailAsync(string email);

        Task<bool> LogoutAsync(string userId);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<string?> GeneratePasswordResetTokenAsync(string email); // Forgot Password
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request);


    }
}
