using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Project.Core.DTO.Places
{
    public class CreatePlaceDto
    {
        [Required(ErrorMessage = "اسم المكان مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // ✅ بقت File مش String
        public IFormFile? CoverImage { get; set; }

        [Required(ErrorMessage = "التصنيف مطلوب")]
        public Guid CategoryId { get; set; }

        public Guid BusinessId { get; set; }

        public List<Guid> TagIds { get; set; } = new List<Guid>();

        public List<OpeningHourDto> OpeningHours { get; set; } = new List<OpeningHourDto>();
    }
}