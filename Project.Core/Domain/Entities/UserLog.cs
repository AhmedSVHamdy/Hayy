using Azure.Storage.Blobs.Models;
using Project.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Project.Core.Domain.Entities
{
    public class UserLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ActionType ActionType { get; set; }
        [BsonRepresentation(BsonType.String)]
        public TargetType TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? TagId { get; set; }
        public int Duration { get; set; }
        public string? SearchQuery { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}