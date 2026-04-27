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
            return await _collection.Find(x => x.UserId == userId).ToListAsync();
        }

        private async Task CreateIndexesAsync()
        {
            var indexManager = _collection.Indexes;

            // 1. Index على الـ UserId (لتسريع جلب توصيات يوزر معين)
            var userIdIndex = Builders<RecommendedItem>.IndexKeys.Ascending(x => x.UserId);
            await indexManager.CreateOneAsync(new CreateIndexModel<RecommendedItem>(userIdIndex));

            // 2. الـ TTL Index (المسح التلقائي بعد 30 يوم)
            var ttlIndex = Builders<RecommendedItem>.IndexKeys.Ascending(x => x.UpdatedAt);
            var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };

            await indexManager.CreateOneAsync(new CreateIndexModel<RecommendedItem>(ttlIndex, ttlOptions));

            // 2. Compound Index على الـ ItemType و الـ ItemId (لتسريع الفلترة بنوع واسم العنصر)
            var typeAndIdIndex = Builders<RecommendedItem>.IndexKeys
                .Ascending(x => x.ItemType)
                .Ascending(x => x.ItemId);
            await indexManager.CreateOneAsync(new CreateIndexModel<RecommendedItem>(typeAndIdIndex));
        }
    }
}
