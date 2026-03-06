using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CreateEventDTO
    {
        public class EventCreateDto
        {
            public Guid PlaceId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string GalleryImages { get; set; } = string.Empty; // JSON String
            public DateTime Datetime { get; set; }
            public int Capacity { get; set; }
            public decimal Price { get; set; }
            public bool IsWaitlistEnabled { get; set; } = true;
            public int WaitlistLimit { get; set; }
        }

        public class EventResponseDto
        {
            public Guid Id { get; set; }
            public Guid PlaceId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime Datetime { get; set; }
            public int Capacity { get; set; }
            public int BookedTickets { get; set; } // عدد التذاكر اللي اتحجزت فعلاً
            public bool IsSoldOut => BookedTickets >= Capacity; // 💡 دي هترجع true لو التذاكر خلصت
            public bool CanJoinWaitlist { get; set; } // عشان الـ Frontend يعرف يفتح زرار الـ Waitlist ولا لأ       
            public decimal Price { get; set; }
            public string Status { get; set; } = string.Empty;
            public bool IsWaitlistEnabled { get; set; }
            public byte[] RowVersion { get; set; }
        }
        public class UpdateEventDto
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string GalleryImages { get; set; } = string.Empty;
            public DateTime Datetime { get; set; }
            public int Capacity { get; set; }
            public decimal Price { get; set; }
            public EventStatus Status { get; set; }
            public bool IsWaitlistEnabled { get; set; }
            public int WaitlistLimit { get; set; }

            // 🛡️ مهم جداً يتبعت عشان الـ Concurrency
            public byte[] RowVersion { get; set; } = null!;
        }
    }
}
