using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public enum LoanStatus
    {
        New, Declined, Accepted, PaidOff
    }
    public enum LoanDuration
    {
        PayAllAtOnce = 0,
        ThreeMonths = 1,
        SixMonths = 2
    }

    [BsonIgnoreExtraElements]
    public class Loan
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
        public EmployeeSummary EmployeeSummary { get; set; }

        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public LoanStatus LoanStatus { get; set; }

        [BsonRequired]
        public decimal LoanDurationInMonths { get; set; }
#nullable enable
        public DateTime? LastPaymentDate { get; set; }
        public string? Reason { get; set; }

        public string? Department { get; set; }
        public string? Position { get; set; }

        [BsonRequired]
        public decimal LoanAmount { get; set; }
        public decimal AmountPayed { get; set; }
        public decimal LastPayment { get; set; }
        [BsonIgnore]
        public decimal LoanOutStandingAmount => LoanAmount - AmountPayed;

#nullable disable

        public DateTime TimeStamp { get; set; }
    }
}
