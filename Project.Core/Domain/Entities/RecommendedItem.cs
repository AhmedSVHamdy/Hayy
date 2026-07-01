using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class RecommendedItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public Guid UserId { get; set; }

        // المكان المقترح
        public Guid PlaceId { get; set; }

        public decimal Score { get; set; }      // درجة التطابق (0-100)
    }
}