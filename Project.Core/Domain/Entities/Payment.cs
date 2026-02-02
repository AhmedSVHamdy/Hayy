using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid BusinessPlanId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string TransactionId { get; set; } = string.Empty;

        public BusinessPlan BusinessPlan { get; set; } = null!;
    }
}

