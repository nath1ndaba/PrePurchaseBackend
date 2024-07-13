using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using System;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public class Rate
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
        public string NameOfPosition { get; set; } 
        public decimal StandardDaysRate { get; set; }
        public decimal SundaysRate { get; set; }
        public decimal SaturdaysRate { get; set; }
        public decimal PublicHolidaysRate { get; set; }
        public decimal DailyBonus { get; set; }
        public decimal OverTimeRate { get; set; }

        public Rate Update(Rate rate)
        {
            NameOfPosition = rate.NameOfPosition;
            StandardDaysRate = rate.StandardDaysRate;
            SundaysRate = rate.SundaysRate;
            SaturdaysRate = rate.SaturdaysRate;
            PublicHolidaysRate = rate.PublicHolidaysRate;
            DailyBonus = rate.DailyBonus;
            OverTimeRate = rate.OverTimeRate;
            return this;
        }

    }

    public enum RateType
    {
        Standard,
        PublicHoliday,
        Saturday,
        Sunday
    }
}

