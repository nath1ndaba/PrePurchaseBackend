using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public class ProcessedPayrollBatch
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
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }
        public string AdminWhoProcessed { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ProcessedBatch Batch { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ProcessedDate { get; set; } = DateTime.Now;
        public List<ProcessedUserPayroll> UserPayroll { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ProcessedUserPayroll
    {

        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId EmployeeId { get; set; }
        [BsonIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; set; }
        [BsonIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Surname { get; set; }
        public bool IsPaid { get; set; } = false;
        public double TotalHours { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalMoneys { get; set; }
    }
}
