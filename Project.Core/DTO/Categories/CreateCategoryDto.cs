using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Project.Core.DTO.Categories
{
    

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "اسم التصنيف مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }
}
