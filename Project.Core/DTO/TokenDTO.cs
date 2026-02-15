using System.ComponentModel.DataAnnotations;

namespace Project.Core.DTO
{
    public class TokenDTO
    {
        [Required]
        public string? AccessToken { get; set; } // ??? ????? Token ??????? ?? AccessToken

        [Required]
        public string? RefreshToken { get; set; }
    }
}