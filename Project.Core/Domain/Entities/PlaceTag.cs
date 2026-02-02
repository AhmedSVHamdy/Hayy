namespace Project.Core.Domain.Entities
{
    public class PlaceTag
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public Guid TagId { get; set; }

        public Place Place { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}

