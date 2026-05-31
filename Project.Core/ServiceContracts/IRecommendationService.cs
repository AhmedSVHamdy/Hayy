using Project.Core.Domain.Entities;
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
        Task<IEnumerable<RecommendedItem>> GetUserRecommendationsAsync(Guid userId);

        /// <summary>
        /// أضف توصية جديدة
        /// </summary>
        Task AddRecommendationAsync(RecommendedItem recommendation);

        /// <summary>
        /// حذف توصية
        /// </summary>
        Task DeleteRecommendationAsync(Guid recommendationId);
    }
}