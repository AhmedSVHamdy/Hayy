using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class BusinessPlan
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BusinessPlanStatus Status { get; set; }                   

        public Business Business { get; set; } = null!;
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}

