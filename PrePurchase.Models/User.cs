using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class User
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

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId EmployeeDetails { get; set; }
        public string EmployeeId { get; set; }
        [BsonRequired]
        public IsSuperAdmin IsSuperAdmin { get; set; }
        [BsonRequired]
        public string Email { get; set; }
        [BsonRequired]
        public string Password { get; set; }

    }

    public enum IsSuperAdmin
    {
        SuperAdmin,
        NormalAdmin,
        Pioneer
    }
}
