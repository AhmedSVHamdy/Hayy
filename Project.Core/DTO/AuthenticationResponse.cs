namespace Project.Core.DTO
{
    public class AuthenticationResponse
    {
        public string? PersonName { get; set; } = string.Empty;
        public Guid? Id { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpirationDateTime { get; set; }

        public string? UserType { get; set; }
        public string? VerificationStatus { get; set; }

        // ✅ الجديد: هل عنده اشتراك نشط؟
        public bool HasActiveSubscription { get; set; }
    }
}