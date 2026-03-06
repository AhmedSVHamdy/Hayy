using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CreateEventBooking
    {
        // 1. الداتا اللي اليوزر بيبعتها عشان يحجز
        public class CreateBookingDto
        {
            public Guid EventId { get; set; }
            public int TicketQuantity { get; set; } = 1; // الديفولت تذكرة واحدة
        }

        // 2. الداتا اللي بترجع لليوزر (التذكرة)
        public class BookingResponseDto
        {
            public Guid Id { get; set; }
            public Guid EventId { get; set; }
            public string EventTitle { get; set; } = string.Empty;
            public int TicketQuantity { get; set; }
            public string Status { get; set; } = string.Empty; // Pending, Confirmed, Waitlisted, Expired
            public decimal TotalAmount { get; set; }
            public DateTime? PaymentDeadline { get; set; }
            public int? WaitlistPosition { get; set; }
            public string BookingCode { get; set; } = string.Empty; // ده اللي هيتعمل بيه الـ QR Code
            public string? QrCodeBase64 { get; set; }
        }
    }
}
