using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Project.Core.Domain.Entities;
using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations.AI___Recommendations_Domain
{
    public static class RecommendationMongo
    {
        public static void RegisterMappings()
        {
            // التأكد إن المابينج متعملش قبل كده عشان ميعملش Exception
            if (!BsonClassMap.IsClassMapRegistered(typeof(RecommendedItem)))
            {
                BsonClassMap.RegisterClassMap<RecommendedItem>(cm =>
                {
                    cm.AutoMap(); // بيعمل مابينج لكل الخصائص أوتوماتيك

                    // تظبيط الـ Primary Key - تحويل ObjectId إلى String
                    cm.MapIdProperty(x => x.Id);

                    // تظبيط الـ Decimal عشان المنجو يفهمه كـ Decimal128 لأعلى دقة
                    cm.MapProperty(x => x.Score)
                      .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                });
            }
        }
    }
}
