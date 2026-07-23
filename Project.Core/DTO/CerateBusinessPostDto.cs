using Microsoft.AspNetCore.Http; // 👈 ضيف النيم سبيس ده
using System;
using System.Text.Json.Serialization;

namespace Project.Core.DTO
{
    public class CerateBusinessPostDto
    {
        public class CreatePostDto
        {
            [JsonIgnore]
            public Guid UserId { get; set; }
            public Guid PlaceId { get; set; }
            public string Content { get; set; } = string.Empty;

            // 👈 استبدلنا الـ string بـ IFormFile
            public IFormFile? ImageFile { get; set; }
        }

        public class PostResponseDto
        {
            public Guid Id { get; set; }
            public string Content { get; set; }
            public string PostAttachments { get; set; } // هيفضل string عشان ده اللي بيرجع كـ Link
            public DateTime CreatedAt { get; set; }
            public string PlaceName { get; set; }
            public string PlaceImage { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public Guid PlaceId { get; set; }
        }

        public class UpdatePostDto
        {
            public string Content { get; set; } = string.Empty;

            // 👈 استبدلنا الـ string بـ IFormFile
            public IFormFile? ImageFile { get; set; }
        }
    }
}