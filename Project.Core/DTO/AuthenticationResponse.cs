namespace Project.Core.DTO
{
    public class AuthenticationResponse
    {
        public string? PersonName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpirationDateTime { get; set; }

        // 👇 الإضافات الجديدة للتوجيه
        public string? UserType { get; set; } // "Admin" or "Business"
        public string? VerificationStatus { get; set; } // "Pending", "Verified", "New", etc.
    }
}




