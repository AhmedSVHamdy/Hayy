namespace Project.Core.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public ICollection<Place> Places { get; set; } = new List<Place>();
        public ICollection<CategoryTag> CategoryTags { get; set; } = new List<CategoryTag>();
    }
}