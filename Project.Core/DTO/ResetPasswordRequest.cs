using System.ComponentModel.DataAnnotations;

namespace Project.Core.DTO
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // التوكن اللي اتبعت في الإيميل
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}