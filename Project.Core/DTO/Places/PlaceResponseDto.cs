using System;
using System.Collections.Generic;
using System.Text;
 using Project.Core.DTO.Tags;
namespace Project.Core.DTO.Places
{
   
    

    public class PlaceResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string CoverImage { get; set; }
        public double Rating { get; set; }

        // اسم التصنيف (بدل الـ ID)
        public string CategoryName { get; set; }

        // تفاصيل الوسوم
        public List<TagDto> Tags { get; set; }

        public List<OpeningHourDto> OpeningHours { get; set; }
    }
}
