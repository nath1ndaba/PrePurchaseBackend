using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models;
using PrePurchase.Models.Converters;
using PrePurchase.Models.Serializers;
using System;
using System.Text.Json.Serialization;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class CompanyShifts
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
        public string ShiftName { get; set; }

        [BsonRequired]
        [BsonSerializer(typeof(TimeOnlyBsonSerializer))]
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly ShiftStartTime { get; set; }
        [BsonRequired]
        [BsonSerializer(typeof(TimeOnlyBsonSerializer))]
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly ShiftEndTime { get; set; }

        public CompanyShifts Update(CompanyShifts shift)
        {
            Id = shift.Id;
            CreatedDate = shift.CreatedDate;
            CreatedBy = shift.CreatedBy;
            UpdatedDate = shift.UpdatedDate;
            UpdatedBy = shift.UpdatedBy;
            DeletedIndicator = shift.DeletedIndicator;
            CompanyId = shift.CompanyId;
            ShiftName = shift.ShiftName;
            ShiftStartTime = shift.ShiftStartTime;
            ShiftEndTime = shift.ShiftEndTime;
            return this;
        }
    }
}
