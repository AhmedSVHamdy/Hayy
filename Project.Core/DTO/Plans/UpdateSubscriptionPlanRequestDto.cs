using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO.Plans
{
    public class UpdateSubscriptionPlanRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string Description { get; set; } = string.Empty;
        public int AiPowerLevel { get; set; }
    }
}
