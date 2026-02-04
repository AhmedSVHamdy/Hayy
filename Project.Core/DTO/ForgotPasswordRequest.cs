using System.ComponentModel.DataAnnotations;

namespace Project.Core.DTO
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}