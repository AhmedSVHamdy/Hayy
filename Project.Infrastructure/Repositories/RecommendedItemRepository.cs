using MongoDB.Driver;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class RecommendedItemRepository : IRecommendedItemRepository
    {
        private readonly IMongoCollection<RecommendedItem> _collection;

        public RecommendedItemRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<RecommendedItem>("RecommendedItems");
            CreateIndexesAsync().GetAwaiter().GetResult();
        }

        // هنا بتكتب الكود الفعلي اللي بيكلم المنجو
        public async Task CreateAsync(RecommendedItem item)
        {
            await _collection.InsertOneAsync(item);
        }

        public async Task<IEnumerable<RecommendedItem>> GetRecommendationsByUserIdAsync(Guid userId)
        {
            return await _collection.Find(x => x.UserId == userId)
                .SortByDescending(x => x.Score)  // ترتيب تنازلي بـ Score (الأفضل أولاً)
                .ToListAsync();
        }

        private async Task CreateIndexesAsync()
        {
            var indexManager = _collection.Indexes;

            // Index على الـ UserId (لتسريع جلب توصيات يوزر معين)
            var userIdIndex = Builders<RecommendedItem>.IndexKeys.Ascending(x => x.UserId);
            await indexManager.CreateOneAsync(new CreateIndexModel<RecommendedItem>(userIdIndex));

            // Index على PlaceId (لتسريع البحث بـ مكان معين)
            var placeIdIndex = Builders<RecommendedItem>.IndexKeys.Ascending(x => x.PlaceId);
            await indexManager.CreateOneAsync(new CreateIndexModel<RecommendedItem>(placeIdIndex));
        }
    }
}
