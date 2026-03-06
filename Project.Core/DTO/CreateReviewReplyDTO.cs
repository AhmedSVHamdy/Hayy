using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CreateReviewReplyDTO
    {
        public class CreateReviewReplyDto
        {
            public Guid ReviewId { get; set; }
            public string ReplyText { get; set; }
            // UserId بناخده من التوكن مش من الـ Body
            public Guid? ReplierId { get; set; }
        }

        public class ReviewReplyResponseDto
        {
            public Guid Id { get; set; }
            public Guid ReviewId { get; set; }
            public string ReplyText { get; set; }
            public Guid ReplierId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
