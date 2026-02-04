using Project.Core.Domain.Entities;

namespace Project.Core.DTO
{
    public class CategoryWithTagsDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<TagDTO> Tags { get; set; } = new();
    }
}






