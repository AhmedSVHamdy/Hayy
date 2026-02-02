namespace Project.Core.Domain.Entities
{
    public class PostLike
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }

        public User User { get; set; } = null!;
        public BusinessPost Post { get; set; } = null!;
    }
}