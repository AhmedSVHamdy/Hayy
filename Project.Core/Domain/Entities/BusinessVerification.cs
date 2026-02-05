using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class BusinessVerification
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string CommercialRegImage { get; set; } = string.Empty;
        public string TaxCardImage { get; set; } = string.Empty;
        public string IdentityCardImage { get; set; } = string.Empty;
        public VerificationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public Guid AdminId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public Business Business { get; set; } = null!;
        public Admin Admin { get; set; } = null!;
    }
}

