using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Project.Core.DTO
{
    public class CeratePostLike
    {
        public class ToggleLikeDto
        {

            public Guid PostId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            public string Title { get; set; } = string.Empty;

        }

        public class LikeResponseDto
        {
            public bool IsLiked { get; set; } // true = أحمر، false = رمادي
            public int LikesCount { get; set; } // العدد الجديد
        }

        public class PostLikeUserDto
        {
            public Guid UserId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string? ProfileImage { get; set; }
        }
    }   
}
