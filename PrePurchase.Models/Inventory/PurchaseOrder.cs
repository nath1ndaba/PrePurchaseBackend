using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.Inventory
{
    [BsonIgnoreExtraElements]
    public class PurchaseOrder
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonRequired]
        public DateTime CreateDate { get; set; }
        [BsonRequired]
        public DateTime UpdateDate { get; set; }

        [BsonRequired]
        public DateTime UpdateteDate { get; set; }

        [BsonRequired]
        public ObjectId CreatedBy { get; set; } //Id
        [BsonRequired]
        public ObjectId UpdatedBy { get; set; }

        [BsonRequired]
        public bool DeletedIndicator { get; set; }

        // Purchase Order Properties
        [BsonRequired]
        public ObjectId ShopId { get; set; }
        public ObjectId SupplierID { get; set; } // Foreign Key
        public string PurchaseOrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal TotalCost { get; set; }
    }
}
