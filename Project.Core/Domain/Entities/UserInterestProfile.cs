namespace Project.Core.Domain.Entities
{
    public class UserInterestProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? TagId { get; set; }
        public decimal InterestScore { get; set; }
        public DateTime LastUpdated { get; set; }

        public User User { get; set; } = null!;
    }
}

