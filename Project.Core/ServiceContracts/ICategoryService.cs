using Project.Core.DTO.Categories;
using Project.Core.DTO.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface ICategoryService
    {
        Task<CategoryWithTagsDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryWithTagsDto?> GetCategoryByIdAsync(Guid id);
        Task<IEnumerable<CategoryWithTagsDto>> GetAllCategoriesAsync();

        // دالة ربط الوسوم
        Task<bool> AssignTagsToCategoryAsync(AssignTagsDto dto);
    }
}
