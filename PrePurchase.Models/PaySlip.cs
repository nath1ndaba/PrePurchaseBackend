using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.LeaveDays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;
using PrePurchase.Models.HistoryModels;

namespace PrePurchase.Models
{
    // the history generates the PaySlip

    [BsonIgnoreExtraElements]
    public class SimplePaySlip
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

        [BsonRequired]
        public DateTime StartDate { get; set; }

        [BsonRequired]
        public DateTime EndDate { get; set; }

        public decimal PayBackLoanAsPerAgreement { get; set; }

        public decimal AccessFunds { get; set; }

        public AllTheLeave Leave { get; set; }
        public AdjustedValuesOnPay AdjustedValuesOnPay { get; set; }


        /// <summary>
        /// Generated using CockData and or bonus
        /// </summary>
        [BsonRequired]
        public List<PaymentInfo> WorkPeriod { get; set; } = new List<PaymentInfo>();

        [BsonIgnore]
        public string Period => $"{StartDate:dd-MMM-yy} to {EndDate:dd-MMM-yy}";

    }

    [BsonIgnoreExtraElements]
    public class PaySlip : SimplePaySlip
    {
        [BsonIgnore]
        public new string Period => $"{StartDate:dd-MMM-yy} to {EndDate:dd-MMM-yy}";

        [BsonIgnore]
        public decimal TotalAmount => WorkPeriod.Sum(w => w.Type == PaymentInfo.PaymentType.Earning ? w.Amount : 0);

        [BsonIgnore]
        public decimal TotalDeductions => WorkPeriod.Sum(w => w.Type == PaymentInfo.PaymentType.TAX || w.Type == PaymentInfo.PaymentType.UIF ? w.Amount : 0);

        [BsonIgnore]
        public int TotalHours => WorkPeriod.Sum(w => w.Type == PaymentInfo.PaymentType.Earning ? w.Hours : 0);

    }
}
