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
    }
}
