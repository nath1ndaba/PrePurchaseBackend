using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.Inventory
{
    [BsonIgnoreExtraElements]
    public class Product
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public DateTime CreateDate { get; set; }
        [BsonRequired]
        public DateTime UpdateDate { get; set; }
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]

        public ObjectId CreatedBy { get; set; } //Id
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]

        public ObjectId UpdatedBy { get; set; }

        [BsonRequired]
        public bool DeletedIndicator { get; set; }

        // Product Properties
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]

        public ObjectId ShopId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]

        public ObjectId CategoryID { get; set; } // Foreign Key
        [JsonConverter(typeof(ObjectIdConverter))]

        public ObjectId SupplierID { get; set; } // Foreign Key
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public int BulkQuantity { get; set; }
        public string BulkUnit { get; set; }
    }
}
