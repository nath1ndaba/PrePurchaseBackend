using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;
using System;
using System.Text.Json.Serialization;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class CompanyDepartments
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CompanyId { get; set; }

        [BsonRequired]
        public string DepartmentName { get; set; }
    }
}
