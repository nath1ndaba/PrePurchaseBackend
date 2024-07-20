using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.Inventory
{
    [BsonIgnoreExtraElements]
    public class Supplier
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

        // Supplier Properties
        [BsonRequired]
        public ObjectId ShopId { get; set; }
        public string SupplierName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
