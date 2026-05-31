using Project.Core.DTO.Places;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IPlaceService
    {
        Task<PlaceResponseDto> CreatePlaceAsync(CreatePlaceDto dto);
        Task<PlaceResponseDto?> GetPlaceByIdAsync(Guid id);
        Task<IEnumerable<PlaceResponseDto>> GetAllPlacesAsync();
        Task<List<PlaceResponseDto>> BasicSearchAsync(string searchTerm, Guid? categoryId);
        Task<IEnumerable<PlaceResponseDto>> GetPlacesByCategoryIdAsync(Guid categoryId);

        // جيب الأماكن الخاصة ببيزنس معين
        Task<IEnumerable<PlaceResponseDto>> GetPlacesByBusinessAsync(Guid businessId);

        // حذف مكان (Soft Delete)
        Task<bool> DeletePlaceAsync(Guid placeId, Guid businessId);

        // تعديل بيانات مكان
        Task<PlaceResponseDto> UpdatePlaceAsync(Guid placeId, Guid businessId, UpdatePlaceDto dto);
    }
}