using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RopositoryContracts
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllWithTagsAsync();
        Task<List<Guid>> GetTopCategoryIdsAsync(int count); // عشان نجيب القيم الافتراضية
    }
}
