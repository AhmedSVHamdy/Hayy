namespace Project.Core.DTO
{
    public class BusinessProfileDTO
    {
        public Guid Id { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string CommercialRegNumber { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string? LogoImage { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public string? RejectionReason { get; set; } // لمعرفة سبب الرفض إن وجد
    }
}
