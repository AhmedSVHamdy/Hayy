using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Core.Domain.Entities
{
    public class Place
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string GalleryImages { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public decimal AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsActive { get; set; }
        public Business Business { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<OpeningHour> OpeningHours { get; set; } = new List<OpeningHour>();
        public ICollection<BusinessPost> BusinessPosts { get; set; } = new List<BusinessPost>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<PlaceTag> PlaceTags { get; set; } = new List<PlaceTag>();
        public ICollection<PlaceFollow> PlaceFollows { get; set; } = new List<PlaceFollow>();
    }
}

