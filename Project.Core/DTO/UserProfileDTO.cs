namespace Project.Core.DTO
{
    public class UserProfileDTO
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; } // ممكن تكون null لو مفيش صورة
        public string? UserType { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}




