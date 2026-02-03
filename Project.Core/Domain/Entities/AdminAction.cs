using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class AdminAction
    {
        public Guid Id { get; set; }
        public Guid AdminId { get; set; }
        public AdminActionType ActionType { get; set; }
        public TargetType TargetType { get; set; }
        public Guid TargetId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Admin Admin { get; set; } = null!;
    }
}