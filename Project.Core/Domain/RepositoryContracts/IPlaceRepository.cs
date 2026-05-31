using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IPlaceRepository : IGenericRepository<Place>
    {
        // دالة تجيب تفاصيل مكان واحد بالكامل (للقراءة فقط)
        Task<Place?> GetByIdWithDetailsAsync(Guid id);

        // دالة تجيب تفاصيل مكان واحد بالكامل (للتعديل والحذف - بدون AsNoTracking)
        Task<Place?> GetByIdWithDetailsForUpdateAsync(Guid id);

        // دالة تجيب كل الأماكن بالتفاصيل (ممكن نستخدمها في الـ Home)
        Task<IEnumerable<Place>> GetAllWithDetailsAsync();

        // دالة تجيب الأماكن الخاصة ببيزنس معين
        Task<IEnumerable<Place>> GetByBusinessIdAsync(Guid businessId);

        Task UpdatePlaceRatingAsync(Guid placeId);
        Task<List<Place>> SearchPlacesAsync(string searchTerm, Guid? categoryId);
        Task<IEnumerable<Place>> GetByCategoryIdAsync(Guid categoryId);
    }
}