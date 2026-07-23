using Microsoft.AspNetCore.Http; // 👈 ضفنا دي عشان الـ IFormFile
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

            // 👈 غيرناها من string لـ IFormFile عشان نستقبل الصورة كملف
            public IFormFile? ImageFile { get; set; }

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
            public int BookedTickets { get; set; }
            public bool IsSoldOut => BookedTickets >= Capacity;
            public bool CanJoinWaitlist { get; set; }
            public decimal Price { get; set; }
            public string Status { get; set; } = string.Empty;
            public bool IsWaitlistEnabled { get; set; }
            public byte[] RowVersion { get; set; }
        }

        public class UpdateEventDto
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

            // 👈 غيرناها هنا كمان لـ IFormFile
            public IFormFile? ImageFile { get; set; }

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