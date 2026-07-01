namespace Project.Core.DTO
{
    public class BusinessAnalyticDTO
    {
        public int TotalViews { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalReviews { get; set; }
        public decimal AvgRating { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
