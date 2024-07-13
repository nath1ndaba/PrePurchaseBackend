using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.Inventory
{
    [BsonIgnoreExtraElements]
    public class StockCount
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public DateTime CreateDate { get; set; }
        [BsonRequired]
        public DateTime UpdateDate { get; set; }
        [BsonRequired]
        public ObjectId CreatedBy { get; set; } //Id
        [BsonRequired]
        public ObjectId UpdatedBy { get; set; }

        [BsonRequired]
        public bool DeletedIndicator { get; set; }

        // Stock Count Properties
        [BsonRequired]
        public ObjectId CompanyId { get; set; }
        public DateTime CountDate { get; set; }
        public string CountType { get; set; }
        public ObjectId ProductID { get; set; } // Foreign Key
        public int CountedQuantity { get; set; }
    }
}
