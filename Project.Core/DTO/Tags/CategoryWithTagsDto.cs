using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO.Tags
{
    public class CategoryWithTagsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // هنا القائمة اللي هترجع للفرونت
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
    }
}
