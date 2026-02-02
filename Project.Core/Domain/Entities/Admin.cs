namespace Project.Core.Domain.Entities
{
    public class Admin
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<BusinessVerification> BusinessVerifications { get; set; } = new List<BusinessVerification>();
    }
}

