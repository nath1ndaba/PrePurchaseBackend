using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using PrePurchase.Models.Converters;
using PrePurchase.Models.Payments;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class Company
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
        public string CompanyName { get; set; }
        public Address Address { get; set; }
        public double DistanceTolerance { get; set; }
        [BsonRequired]
        public string RegisterationNumber { get; set; }
        public string Email { get; set; }
        public string CellNumber { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsLoanActive { get; set; }
        public bool IsHiddenEnv { get; set; }
        public bool IsMobileClockingActive { get; set; }
        public bool IsSupplierPaymentActive { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<string> Positions { get; set; } = new();
        public AccountBalanceModel AccountBalance { get; set; } = new();
        public List<Project> Project { get; set; }

        [BsonRequired]
        //[JsonInclude]
        public List<Shift> Shifts = new();
        [BsonRequired]
        //[JsonInclude]
        public List<Deduction> Deductions = new();
        //[JsonInclude]
        public List<Rate> Rates = new();
        //[JsonInclude]
        public List<string> SuppliersPaymentMethods = new();

        public Rate GetRateForPosition(string position)
        {
            return Rates.Where(x => x.NameOfPosition.ToLowerInvariant() == position.ToLowerInvariant()).First();
        }

        public Rate GetRateForPosition(CompanyEmployee companyEmployee)
            => GetRateForPosition(companyEmployee);

    }

}
