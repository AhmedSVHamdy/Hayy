using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CreateReviewDto
    {
        public Guid PlaceId { get; set; }
        // الـ UserId هنجيبه من التوكن في الكنترولر، بس خليه هنا للاحتياط
        public Guid UserId { get; set; }
        public int Rating { get; set; } // من 1 لـ 5
        public string? Comment { get; set; }
        public string? ReviewImages { get; set; } // لينك الصورة أو Base64
    }

    // شنطة العرض (Response)
    public class ReviewResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // عشان نعرض اسم اللي كتب التعليق
        public Guid PlaceId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string ReviewImages { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
