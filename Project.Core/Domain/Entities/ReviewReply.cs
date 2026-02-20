using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.Entities
{
    public class ReviewReply
    {
        public Guid Id { get; set; }
        public string ReplyText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // العلاقة مع الريفيو
        public Guid ReviewId { get; set; }
        public Review Review { get; set; }

        // مين اللي رد؟ (غالباً صاحب المكان أو أدمن)
        public Guid ReplierId { get; set; }
        // public User Replier { get; set; } // لو عندك User Entity
    }
}
