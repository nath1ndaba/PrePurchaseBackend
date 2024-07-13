using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models;
using PrePurchase.Models.Converters;
using System;
using System.Text.Json.Serialization;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class CompanyRates
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

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CompanyPositionId { get; set; }
        public decimal StandardDaysRate { get; set; }
        public decimal SundaysRate { get; set; }
        public decimal SaturdaysRate { get; set; }
        public decimal PublicHolidaysRate { get; set; }
        public decimal DailyBonus { get; set; }
        public decimal OverTimeRate { get; set; }

        public CompanyRates Update(CompanyRates rate)
        {
            Id = rate.Id;
            CreatedDate = rate.CreatedDate;
            CreatedBy = rate.CreatedBy;
            UpdatedDate = rate.UpdatedDate;
            UpdatedBy = rate.UpdatedBy;
            DeletedIndicator = rate.DeletedIndicator;
            CompanyId = rate.CompanyId;
            CompanyPositionId = rate.CompanyPositionId;
            StandardDaysRate = rate.StandardDaysRate;
            SundaysRate = rate.SundaysRate;
            SaturdaysRate = rate.SaturdaysRate;
            PublicHolidaysRate = rate.PublicHolidaysRate;
            DailyBonus = rate.DailyBonus;
            OverTimeRate = rate.OverTimeRate;
            return this;
        }
    }
}
