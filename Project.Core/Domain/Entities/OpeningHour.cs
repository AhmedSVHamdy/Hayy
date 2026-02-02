using Project.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Core.Domain.Entities
{
    public class OpeningHour
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public DayOfWeekEnum DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        public Place Place { get; set; } = null!;
    }
}

