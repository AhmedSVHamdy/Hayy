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
        // 👇 التغيير هنا: إرجاع RegisterResponse
        Task<RegisterResponse> RegisterBusinessAsync(RegisterDTO registerDTO, IFormFile? image);
        Task<RegisterResponse> RegisterAdminAsync(RegisterDTO registerDTO, IFormFile? image);

        // 👇 Login بيرجع AuthenticationResponse (فيه التوكن)
        Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO);

        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task ResendConfirmationEmailAsync(string email);
        Task<bool> LogoutAsync(string userId);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);

        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request);

        Task<bool> MakeAdmin(string email);

        Task<AuthenticationResponse> RefreshTokenAsync(TokenDTO tokenDTO);

        Task<UserProfileDTO> GetUserProfileAsync(string userId);

        Task<bool> CheckEmailExistsAsync(string email);

        Task<bool> RevokeTokenAsync(string token);
    }
}
