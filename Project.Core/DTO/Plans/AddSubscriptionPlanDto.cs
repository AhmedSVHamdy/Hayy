using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Project.Core.DTO.Plans
{
    public class AddSubscriptionPlanRequestDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "DurationDays is required.")]
        [Range(1, 3650, ErrorMessage = "DurationDays must be between 1 and 3650 days.")]
        public int DurationDays { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "AiPowerLevel is required.")]
        [Range(1, 10, ErrorMessage = "AiPowerLevel must be between 1 and 10.")]
        public int AiPowerLevel { get; set; }
    }
}
