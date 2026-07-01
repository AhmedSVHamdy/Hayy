using Microsoft.AspNetCore.Http;

namespace Project.Core.DTO
{
    public class UpdateBusinessProfileDTO
    {
        public string BrandName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;

        // اللوجو اختياري في التعديل (لو مبعثوش هيفضل القديم زي ما هو)
        public IFormFile? NewLogoImage { get; set; }
    }
}
