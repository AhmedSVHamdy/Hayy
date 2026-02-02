using Microsoft.Extensions.Logging;
using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? ProfileImage { get; set; }
        public UserType UserType { get; set; }
        public string? City { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserSettings? UserSettings { get; set; }
        public ICollection<Business> Businesses { get; set; } = new List<Business>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
        public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public ICollection<EventBooking> EventBookings { get; set; } = new List<EventBooking>();
        public ICollection<PlaceFollow> PlaceFollows { get; set; } = new List<PlaceFollow>();
        public ICollection<UserLog> UserLogs { get; set; } = new List<UserLog>();
        public ICollection<UserInterestProfile> UserInterestProfiles { get; set; } = new List<UserInterestProfile>();
        public ICollection<RecommendedItem> RecommendedItems { get; set; } = new List<RecommendedItem>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}