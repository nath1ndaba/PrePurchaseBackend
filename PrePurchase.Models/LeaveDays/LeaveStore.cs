using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.LeaveDays
{

    [BsonIgnoreExtraElements]
    public class LeaveStore
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; }
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
        [BsonRequired]
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        [BsonRequired]
        public decimal AnnualLeaveDays { get; set; }
        public decimal SickLeaveDays { get; set; }
        public decimal FamilyLeaveDays { get; set; }
        public DateTime TimeStamp { get; set; } = new DateTime();
    }
}
