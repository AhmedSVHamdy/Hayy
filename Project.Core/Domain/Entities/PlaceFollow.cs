namespace Project.Core.Domain.Entities
{
    public class PlaceFollow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PlaceId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Place Place { get; set; } = null!;
    }
}