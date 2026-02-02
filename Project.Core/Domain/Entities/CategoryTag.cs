namespace Project.Core.Domain.Entities
{
    public class CategoryTag
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Guid TagId { get; set; }

        public Category Category { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}

