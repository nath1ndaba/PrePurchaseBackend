using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    /*
     * Time Period | Description    | Amount
     * ------------|----------------|-------
     *             |                |
     */

    [BsonIgnoreExtraElements]
    public class PaymentInfo
    {
        
        public enum PaymentType
        {
            Earning, UIF, TAX
        }

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

        public string Description { get; set; }
        public RateType RateType { get; set; } 

        public decimal Amount { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [BsonRepresentation(BsonType.String)]
        public PaymentType Type { get; set; } = PaymentType.Earning;

        /// <summary>
        /// A date string of two dates joined by - (dash)
        /// startdate-enddate e.g 1/12/2020 08:00-1/01/2021 08:00
        /// </summary>
        /// 
        [BsonIgnore]
        public string Period => GenereratePeriod(StartTime, EndTime);

        [BsonIgnore]
        public int Hours => (EndTime-StartTime).Hours;

        [BsonIgnore]
        private const string _dateFormat = "dd/MM/yyyy HH:mm";

        public static string GenereratePeriod(DateTime start, DateTime end)
        {
            return $"{start.ToString(_dateFormat)}-{end.ToString(_dateFormat)}";
        }

        public static implicit operator PaymentInfo(ClockData clockData)
        {
            clockData = ClockData.Normalize(clockData);
            return new PaymentInfo
            {
                Amount = clockData.Amount,
                StartTime = clockData.ClockIn,
                EndTime = clockData.ClockOut!.Value
            };
        }
    }
}
