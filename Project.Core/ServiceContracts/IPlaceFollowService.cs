using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePlaceFollow;

namespace Project.Core.ServiceContracts
{
    public interface IPlaceFollowService
    {
        Task<bool> ToggleFollowAsync(Guid userId, TogglePlaceFollowDto dto);

        Task<PagedResult<PlaceFollowResponseDto>> GetFollowersByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize);

        Task<PagedResult<PlaceFollowResponseDto>> GetFollowedPlacesByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize);
    }
}
