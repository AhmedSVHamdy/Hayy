namespace Project.Core.Domain.Entities
{
    public class PostComment
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public BusinessPost Post { get; set; } = null!;
        public User User { get; set; } = null!;
        public PostComment? ParentComment { get; set; }
        public ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
    }
}