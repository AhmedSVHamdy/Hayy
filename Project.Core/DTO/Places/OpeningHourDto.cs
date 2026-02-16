using System;
using System.Collections.Generic;
using System.Text;
 using Project.Core.Enums; // تأكد ان الـ Enum ده موجود عندك 
namespace Project.Core.DTO.Places
{
   

    public class OpeningHourDto
    {
        public DayOfWeekEnum DayOfWeek { get; set; } // السبت، الأحد...
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }
}
