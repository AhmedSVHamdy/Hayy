using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class UserLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // ممكن تشيلها لو مش عايز ترجعها
        public string ActionType { get; set; } // هنرجعها String عشان الفرونت يفهمها أسهل
        public string TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public int Duration { get; set; }
        public string? SearchQuery { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
