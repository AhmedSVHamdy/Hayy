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
        public List<Guid> TagId { get; set; } = new List<Guid>();
        public string? Details { get; set; }
        public string? SearchQuery { get; set; }
    }
    public class LogSearchRequestDto
    {
        public Guid? UserId { get; set; }
        public string SearchTerm { get; set; } // الكلمة اللي كتبها (مثال: "كافيه")
        public Guid? CategoryId { get; set; }  // لو كان مختار كاتيجيوري معينة وهو بيبحث
        public List<Guid>? TagId { get; set; } // لو كان مختار Tags
    }
}
