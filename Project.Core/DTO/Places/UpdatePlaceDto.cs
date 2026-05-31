using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO.Places
{
    public class UpdatePlaceDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CoverImage { get; set; }
        public Guid? CategoryId { get; set; }

        // لو مش عايز تعدل التاجز، ابعت null — لو عايز تمسحهم كلهم ابعت قائمة فاضية
        public List<Guid>? TagIds { get; set; }

        // لو مش عايز تعدل المواعيد، ابعت null — لو عايز تمسحهم كلهم ابعت قائمة فاضية
        public List<OpeningHourDto>? OpeningHours { get; set; }
    }
}