
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IRecommendedItemRepository _recommendedItemRepository;

        public RecommendationService(IRecommendedItemRepository recommendedItemRepository)
        {
            _recommendedItemRepository = recommendedItemRepository;
        }

        /// <summary>
        /// جيب التوصيات من Repository لليوزر
        /// </summary>
        public async Task<IEnumerable<RecommendedItem>> GetUserRecommendationsAsync(Guid userId)
        {
            return await _recommendedItemRepository.GetRecommendationsByUserIdAsync(userId);
        }

        /// <summary>
        /// أضف توصية جديدة
        /// </summary>
        public async Task AddRecommendationAsync(RecommendedItem recommendation)
        {
            if (recommendation == null)
                throw new ArgumentNullException(nameof(recommendation));

            if (recommendation.Score < 0 || recommendation.Score > 1)
                throw new ArgumentException("Score must be between 0 and 1");

            recommendation.CreatedAt = DateTime.UtcNow;
            recommendation.UpdatedAt = DateTime.UtcNow;

            await _recommendedItemRepository.CreateAsync(recommendation);
        }

        /// <summary>
        /// حذف توصية
        /// </summary>
        public async Task DeleteRecommendationAsync(Guid recommendationId)
        {
            if (recommendationId == Guid.Empty)
                throw new ArgumentException("Recommendation ID is required");

            // تحتاج تضيف دالة Delete في الـ Repository إذا ما كانت موجودة
            // await _recommendedItemRepository.DeleteAsync(recommendationId);
        }
    }
}