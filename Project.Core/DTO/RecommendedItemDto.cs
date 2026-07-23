namespace Project.Core.DTO
{
    public class RecommendedItemDto
    {
        
            public string PlaceId { get; set; }
            public string Name { get; set; }
            public string City { get; set; }
            public double Score { get; set; }
            public string CategoryId { get; set; }
        
    }
    public class AiRecommendationResponse
    {
        public string UserId { get; set; }
        public int Count { get; set; }
        public List<RecommendedItemDto> Data { get; set; }
    }
}