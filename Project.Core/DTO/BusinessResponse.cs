using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class BusinessResponse
    {
        public Guid Id { get; set; }

        // بيانات صاحب المحل (مهمة للأدمن)
        public Guid UserId { get; set; }
        public string OwnerName { get; set; } = string.Empty; // هنجيبها من جدول User

        // بيانات المحل الأساسية
        public string BrandName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string LogoImage { get; set; } = string.Empty;

        // بيانات قانونية (عشان الأدمن يراجعها)
        public string CommercialRegNumber { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;

        // حالة التوثيق (أهم حاجة)
        public string VerificationStatus { get; set; } = string.Empty; // هنرجعها نص (Verified/Pending)

        // تواريخ
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; } // تاريخ الموافقة
    }
}
