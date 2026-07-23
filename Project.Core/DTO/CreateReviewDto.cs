using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http; // 👈 ضفنا دي عشان الـ IFormFile

namespace Project.Core.DTO
{
    public class CreateReviewDto
    {
        public Guid PlaceId { get; set; }
        // الـ UserId هنجيبه من التوكن في الكنترولر، بس خليه هنا للاحتياط
        public Guid UserId { get; set; }
        public int Rating { get; set; } // من 1 لـ 5
        public string? Comment { get; set; }

        // 👈 التعديل هنا: غيرناها لـ ImageFile من نوع IFormFile
        public IFormFile? ImageFile { get; set; }
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
    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        // 👈 التعديل هنا كمان
        public IFormFile? ImageFile { get; set; }
    }
}