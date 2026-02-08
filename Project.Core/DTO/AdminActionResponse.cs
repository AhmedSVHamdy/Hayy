namespace Project.Core.DTO
{
    public class AdminActionResponse
    {
        public Guid Id { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // هنرجعها كنص
        public string TargetType { get; set; } = string.Empty; // هنرجعها كنص
        public string TargetId { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}




