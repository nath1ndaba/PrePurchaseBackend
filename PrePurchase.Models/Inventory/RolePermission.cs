using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.Inventory
{
    [BsonIgnoreExtraElements]
    public class RolePermission
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

        // Role-Permission Properties
        public ObjectId RoleID { get; set; } // Foreign Key
        public ObjectId PermissionID { get; set; } // Foreign Key
        [BsonRequired]
        public ObjectId ShopId { get; set; }
    }
}
