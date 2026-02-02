namespace Project.Core.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PlaceId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string ReviewImages { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Place Place { get; set; } = null!;
    }
}