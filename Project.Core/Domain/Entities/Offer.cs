using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class Offer
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GalleryImages { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OfferStatus Status { get; set; }

        public Place Place { get; set; } = null!;
    }
}

