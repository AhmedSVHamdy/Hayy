using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IPlaceFollowRepository
    {
        Task<PlaceFollow?> GetFollowAsync(Guid userId, Guid placeId);
        Task AddAsync(PlaceFollow placeFollow);
        Task RemoveAsync(PlaceFollow placeFollow);
        // تعديل للباجينيشن: بنرجع الداتا + العدد الكلي
        Task<(IEnumerable<PlaceFollow> Items, int TotalCount)> GetFollowersByPlaceIdAsync(Guid placeId, int page, int size);
        Task<(IEnumerable<PlaceFollow> Items, int TotalCount)> GetFollowedPlacesByUserIdAsync(Guid userId, int page, int size);
    }
}
