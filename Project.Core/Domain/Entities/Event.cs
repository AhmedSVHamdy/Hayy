using Project.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Core.Domain.Entities
{
    public class Event
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GalleryImages { get; set; } = string.Empty;
        public DateTime Datetime { get; set; }
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Active; // حالة الإيفنت (نشط أو ملغي) [cite: 40, 52]

        // 💡 إضافات لدعم الـ Waitlist والـ Concurrency
        public bool IsWaitlistEnabled { get; set; } = true; // هل مسموح بقائمة انتظار؟
        public int WaitlistLimit { get; set; } // أقصى عدد يدخل قائمة الانتظار

        // 🛡️ حماية ضد الـ Race Condition (Optimistic Concurrency)
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public Place Place { get; set; } = null!;
        public ICollection<EventBooking> EventBookings { get; set; } = new List<EventBooking>();
    }
}

