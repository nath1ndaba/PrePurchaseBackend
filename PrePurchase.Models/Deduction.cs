using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using System;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public enum AmountType
    {
        Money,
        Percentage,
        Loan,
        Paye_Tax,
        Pension
    }
    public class Deduction
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
        public string TypeOfDeduction { get; set; } 
        public decimal AmountToDeduct { get; set; }
        [BsonRepresentation(BsonType.String)]
        public AmountType AmountType { get; set; } = AmountType.Money;

        public Deduction Update(Deduction deduction)
        {
            TypeOfDeduction = deduction.TypeOfDeduction;
            AmountToDeduct = deduction.AmountToDeduct;
            AmountType = deduction.AmountType;

            return this;
        }
    }
}

