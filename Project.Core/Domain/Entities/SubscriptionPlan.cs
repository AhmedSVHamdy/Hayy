namespace Project.Core.Domain.Entities
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string Description { get; set; } = string.Empty;
        public int AiPowerLevel { get; set; }
        public bool IsActive { get; set; }

        public ICollection<BusinessPlan> BusinessPlans { get; set; } = new List<BusinessPlan>();
    }
}

