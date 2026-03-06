using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CreateUserLogDto
    {
        // الـ UserId هنجيبه من التوكن في الكنترولر، بس ممكن تبعته هنا مؤقتاً
        public Guid UserId { get; set; }
        public ActionType ActionType { get; set; }
        public TargetType TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? TagId { get; set; }
        public int Duration { get; set; } // بالثواني
        public string? Details { get; set; }
    }
}
