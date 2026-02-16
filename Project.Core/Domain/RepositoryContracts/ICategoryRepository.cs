using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<Category>> GetAllWithTagsAsync();
        Task<List<Guid>> GetTopCategoryIdsAsync(int count); // عشان نجيب القيم الافتراضية

        Task<Category?> GetByIdWithTagsAsync(Guid id);// دالة خاصة تجيب التصنيف شامل (tags)
    }
}
