using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO.Tags
{
    public class AssignTagsDto
    {
        public Guid CategoryId { get; set; }
        public List<Guid> TagIds { get; set; } = new List<Guid>();
    }
}
