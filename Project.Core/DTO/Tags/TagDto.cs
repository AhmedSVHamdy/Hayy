using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO.Tags
{
    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }
}
