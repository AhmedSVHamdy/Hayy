using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class RecommendedItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ItemType ItemType { get; set; }
        public Guid ItemId { get; set; }
        public decimal Score { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}

