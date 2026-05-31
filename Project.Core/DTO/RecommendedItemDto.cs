namespace Project.Core.DTO
{
    public class RecommendedItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? PlaceId { get; set; }           // مكان مقترح
        public Guid? EventId { get; set; }           // حدث مقترح
        public Guid? PostId { get; set; }            // بوست مقترح
        public string RecommendationType { get; set; } // "Place", "Event", "Post"
        public string Reason { get; set; }           // سبب التوصية
        public double Score { get; set; }            // درجة التطابق
        public DateTime CreatedAt { get; set; }

        // معلومات إضافية للعرض
        public string? PlaceName { get; set; }
        public string? EventTitle { get; set; }
        public string? PostContent { get; set; }
    }
}