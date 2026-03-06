using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CeratePlaceFollow
    {
        public class TogglePlaceFollowDto
        {
            public Guid PlaceId { get; set; }
        }

        // بنرجع بيه الداتا (لو عايزين نعرض ليستة المتابعين)
        public class PlaceFollowResponseDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string UserName { get; set; } // هنجيبها من جدول اليوزر
            public Guid PlaceId { get; set; }
            public string PlaceName { get; set; } // هنجيبها من جدول الأماكن
            public DateTime CreatedAt { get; set; }
        }
        
    }
}
