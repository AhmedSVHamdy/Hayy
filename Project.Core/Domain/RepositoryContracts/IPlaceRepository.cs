using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IPlaceRepository : IGenericRepository<Place>
    {
        // دالة تجيب تفاصيل مكان واحد بالكامل
        Task<Place?> GetByIdWithDetailsAsync(Guid id);

        // دالة تجيب كل الأماكن بالتفاصيل (ممكن نستخدمها في الـ Home)
        Task<IEnumerable<Place>> GetAllWithDetailsAsync();
    }
}
