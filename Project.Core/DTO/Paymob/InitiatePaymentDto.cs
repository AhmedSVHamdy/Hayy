namespace Project.Core.DTOs.Payments
{
    public class InitiatePaymentDto
    {
        public Guid BusinessId { get; set; }
        public Guid PlanId { get; set; }
    }
}