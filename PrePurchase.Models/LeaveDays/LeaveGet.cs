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
    public class LeaveGet
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

       public decimal AnnualLeaveDays { get; set; }
        public decimal SickLeaveDays { get; set; }
        public decimal FamilyLeaveDays { get; set; }
    }
}
