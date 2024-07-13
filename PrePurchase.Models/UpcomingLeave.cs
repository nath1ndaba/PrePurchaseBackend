using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public class UpcomingLeave
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
        public string Name { get; set; }
        public string TypeOfLeave { get; set; }
        public DateTime ShiftEndTime { get; set; }
    }
}

