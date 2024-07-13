using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class Leave
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }

        [BsonRequired]
        public EmployeeSummary EmployeeSummary { get; set; }

        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }

        public string Department { get; set; }
        public string Position { get; set; }

        [BsonRequired]
        public TypeOfLeave TypeOfLeave { get; set; }
        [BsonRequired]
        public int DaysToTake { get; set; }
        [BsonRequired]
        public DateTime LeaveStartDate { get; set; }
        [BsonRequired]
        public DateTime LeaveEndDate { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public LeaveStatus Status { get; set; }

#nullable enable
        public string? Comment { get; set; }
#nullable disable
        public DateTime TimeStamp { get; set; }
    }
}

