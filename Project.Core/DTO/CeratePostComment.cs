using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CeratePostComment
    {
        public class CreateCommentDto
        {
            public Guid PostId { get; set; }
            public Guid UserId { get; set; }
            public Guid? ParentCommentId { get; set; }
            public string Content { get; set; } = string.Empty;



        }
        public class CommentResponseDto
        {
            public Guid Id { get; set; }
            public Guid PostId { get; set; }
            public Guid UserId { get; set; }
            public Guid? ParentCommentId { get; set; }
            public string Content { get; set; } = string.Empty;
            public string UserName { get; set; } // اسم المستخدم          
            public string UserImage { get; set; } // صورة المستخدم
            public DateTime CreatedAt { get; set; }

            public List<CommentResponseDto> Replies { get; set; } = new List<CommentResponseDto>(); // الردود على التعليق
        }





    }
        
            
    
    
    
}
