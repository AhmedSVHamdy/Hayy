using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class EventBooking
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public int TicketQuantity { get; set; }
        public int CheckedInCount { get; set; }
        public BookingStatus Status { get; set; }
        public bool IsPaid { get; set; }
        public decimal PaidAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;

        public User User { get; set; } = null!;
        public Event Event { get; set; } = null!;
    }
}