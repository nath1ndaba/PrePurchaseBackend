using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;
using PrePurchase.Models.Serializers;

namespace PrePurchase.Models
{
    public class Shift
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
        public string Name { get; set; }
        [BsonRequired]
        [BsonSerializer(typeof(TimeOnlyBsonSerializer))]
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly ShiftStartTime { get; set; } 
        [BsonRequired]
        [BsonSerializer(typeof(TimeOnlyBsonSerializer))]
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly ShiftEndTime { get; set; }

        public Shift Update(Shift shift)
        {
            Name = shift.Name;
            ShiftStartTime = shift.ShiftStartTime;
            ShiftEndTime = shift.ShiftEndTime;
            return this;
        }
      
    }

}
 
