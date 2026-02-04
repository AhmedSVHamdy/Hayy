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
        public Task<User> Register(RegisterDTO registerDTO, IFormFile? image);
        public Task<User> Login(LoginDTO loginDTO);
        Task<bool> Logout(string userId);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);

        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);

        Task ResendConfirmationEmailAsync(string email);
    }

}
