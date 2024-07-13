using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models;
using PrePurchase.Models.Converters;
using System;
using System.Text.Json.Serialization;


namespace PrePurchase.Models.Extractors
{
    [BsonIgnoreExtraElements]
    public class SimpleHistory
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
        [BsonRequired]
        public ObjectId TimeSummaryId { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonRequired]
        public ObjectId EmployeeDetailsId { get; set; }
        [BsonRequired]
        public string EmployeeId { get; set; }
        [BsonRequired]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string BatchCode { get; set; }
        [BsonIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; set; }
        [BsonIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Surname { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonRequired]
        public ObjectId CompanyId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsPaid { get; set; }

        [BsonIgnore]
        public string Month => Timestamp.ToMonthName();
    }
}


