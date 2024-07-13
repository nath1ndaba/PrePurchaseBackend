using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public record RefreshToken
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }
        [BsonRequired]
        public string Token { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Invalidated { get; set; }

        public static string GenerateNewId() 
            => ObjectId.GenerateNewId().ToString();

        public static RefreshToken WithId(string id)
        {
            return new RefreshToken { Id = ObjectId.Parse(id) };
        }
    }
}
