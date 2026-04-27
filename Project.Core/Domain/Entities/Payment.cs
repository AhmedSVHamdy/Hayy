using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        public Guid? SubscriptionId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

        public PaymentMethod PaymentMethod { get; set; }  // Enum بدل string
        public PaymentStatus Status { get; set; }         // Enum بدل string

        public long? PaymobOrderId { get; set; }          // رقم الأوردر عند بايموب
        public long? PaymobTransactionId { get; set; }    // رقم العملية المرجعي

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public Guid BusinessId { get; set; }
        public Guid PlanId { get; set; }

        public BusinessSubscription? Subscription { get; set; }
    }
}