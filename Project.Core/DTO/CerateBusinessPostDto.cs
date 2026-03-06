using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Project.Core.DTO
{
    public class CerateBusinessPostDto
    {
        // 1. الداتا اللي المطعم هيبعتها (Input)
        public class CreatePostDto
        {
            [JsonIgnore]
            public Guid UserId { get; set; }
            public Guid PlaceId { get; set; }
            public string Content { get; set; } = string.Empty;
            public string? PostAttachments { get; set; } // رابط الصورة أو الفيديو
        }

        // 2. الداتا اللي الناس هتشوفها (Output)
        public class PostResponseDto
        {
            public Guid Id { get; set; }
            public string Content { get; set; }
            public string PostAttachments { get; set; }
            public DateTime CreatedAt { get; set; }

            // معلومات إضافية للعرض
            public string PlaceName { get; set; } // اسم المطعم
            public string PlaceImage { get; set; } // لوجو المطعم
            public int LikesCount { get; set; }   // عدد اللايكات
            public int CommentsCount { get; set; } // عدد الكومنتات
            public Guid PlaceId { get; set; }
        }
    }
}
