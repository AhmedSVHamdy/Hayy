namespace Project.Core.Domain.Entities
{
    public class Event
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GalleryImages { get; set; } = string.Empty;
        public DateTime Datetime { get; set; }
        public int Capacity { get; set; }
        public decimal Price { get; set; }

        public Place Place { get; set; } = null!;
        public ICollection<EventBooking> EventBookings { get; set; } = new List<EventBooking>();
    }
}

