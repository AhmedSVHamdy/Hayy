using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Core.ServiceContracts
{
    public interface IRecommendationService
    {
        /// <summary>
        /// جيب التوصيات لليوزر
        /// </summary>
        Task<IEnumerable<RecommendedItemDto>> GetUserRecommendationsAsync(Guid userId);

    }
}