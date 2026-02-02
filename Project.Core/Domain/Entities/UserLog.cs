using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class UserLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ActionType ActionType { get; set; }
        public TargetType TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? TagId { get; set; }
        public int Duration { get; set; }
        public string? SearchQuery { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}