using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Project.Core.DTO.Places
{
    public class UpdatePlaceDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // ✅ بقت File مش String
        public IFormFile? CoverImage { get; set; }

        public Guid? CategoryId { get; set; }

        public List<Guid>? TagIds { get; set; }

        public List<OpeningHourDto>? OpeningHours { get; set; }
    }
}