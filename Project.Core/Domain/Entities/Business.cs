using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class Business
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string CommercialRegNumber { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public VerificationStatus VerificationStatus { get; set; }
        public string LogoImage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Place> Places { get; set; } = new List<Place>();
        public ICollection<BusinessPlan> BusinessPlans { get; set; } = new List<BusinessPlan>();
        public BusinessAnalytic? BusinessAnalytics { get; set; }

        // 👇👇 التغيير هنا: خليناها List وغيرنا الاسم لـ Verifications عشان يبقى سهل 👇👇
        public virtual ICollection<BusinessVerification> Verifications { get; set; } = new List<BusinessVerification>();
    }

}