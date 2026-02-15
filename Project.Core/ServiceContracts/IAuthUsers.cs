using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IAuthUsers
    {
        Task<RegisterResponse> Register(RegisterDTO registerDTO, IFormFile? image);

        // 👇 التغيير: بترجع AuthenticationResponse بدل User
        Task<AuthenticationResponse> Login(LoginDTO loginDTO);

        // باقي الدوال زي ما هي أو ممكن نلفها بـ Response عام لو حبيت
        Task<bool> Logout(string userId);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task ResendConfirmationEmailAsync(string email);

        Task<AuthenticationResponse> RefreshTokenAsync(TokenDTO tokenModel);

        Task<UserProfileDTO> GetUserProfileAsync(string userId);

        Task<bool> DeleteAccountAsync(string userId);


        Task<AuthenticationResponse> GoogleLoginAsync(SocialLoginDTO request, string role = "User");
    }

}
