using Project.Core.Enums;
using System;

namespace Project.Core.Domain.Entities
{
    public class EventPayment
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!; // Navigation Property

        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!; // Navigation Property

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }

        public long? PaymobOrderId { get; set; }
        public long? PaymobTransactionId { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}