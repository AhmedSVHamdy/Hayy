using Microsoft.AspNetCore.Http;

namespace Project.Core.DTO
{
    public class BusinessOnboardingDTO
    {
        public string BrandName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string CommercialRegNumber { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;

        // ملفات الصور اللي هتترفع
        public IFormFile LogoImage { get; set; } = null!;
        public IFormFile CommercialRegImage { get; set; } = null!;
        public IFormFile TaxCardImage { get; set; } = null!;
        public IFormFile IdentityCardImage { get; set; } = null!;
    }
}
