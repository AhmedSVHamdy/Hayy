using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IRecommendedItemRepository
    {
        // مجرد أمثلة للميثودز اللي هتحتاجها
        Task CreateAsync(RecommendedItem item);
        Task<IEnumerable<RecommendedItem>> GetRecommendationsByUserIdAsync(Guid userId);
        // ... أي ميثود تانية هتحتاجها
    }
}
