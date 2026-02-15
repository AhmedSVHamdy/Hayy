namespace Project.Core.DTO
{
    public class BusinessVerificationSummaryDTO
    {
        public Guid BusinessId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string CommercialRegNumber { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public string? CommercialRegImage { get; set; }
    }
}




