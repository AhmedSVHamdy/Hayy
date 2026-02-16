using System;
using System.Collections.Generic;
using System.Text;
 using System.ComponentModel.DataAnnotations;

namespace Project.Core.DTO.Places
{
   

    public class CreatePlaceDto
    {
        [Required(ErrorMessage = "اسم المكان مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? CoverImage { get; set; }

        [Required(ErrorMessage = "التصنيف مطلوب")]
        public Guid CategoryId { get; set; }
        //البزنس مربوط مع المكان
        public Guid BusinessId { get; set; }

        // قائمة بمعرفات الوسوم (Tags)
        public List<Guid> TagIds { get; set; } = new List<Guid>();

        // ساعات العمل
        public List<OpeningHourDto> OpeningHours { get; set; } = new List<OpeningHourDto>();
    }
}
