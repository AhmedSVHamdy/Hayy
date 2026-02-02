namespace Project.Core.Domain.Entities
{
    public class BusinessPost
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string PostAttachments { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public Place Place { get; set; } = null!;
        public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
        public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    }
}

