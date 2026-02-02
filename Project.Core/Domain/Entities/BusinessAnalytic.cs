namespace Project.Core.Domain.Entities
{
    public class BusinessAnalytic
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public int TotalViews { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalReviews { get; set; }
        public decimal AvgRating { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public DateTime LastUpdated { get; set; }

        public Business Business { get; set; } = null!;
    }
}

