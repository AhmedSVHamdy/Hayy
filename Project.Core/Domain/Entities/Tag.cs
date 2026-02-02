namespace Project.Core.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public ICollection<CategoryTag> CategoryTags { get; set; } = new List<CategoryTag>();
        public ICollection<PlaceTag> PlaceTags { get; set; } = new List<PlaceTag>();
    }
}

