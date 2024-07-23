using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;

public class Recharge
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId UpdatedBy { get; set; }

    public bool DeletedIndicator { get; set; }

    [Required]
    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
    public DateTime RechargeDate { get; set; }
    public RechargeStatus Status { get; set; } // "Pending", "Completed", "Failed"
    public string PaymentMethod { get; set; } // "CreditCard", "DebitCard", "BankTransfer", etc.
}
public enum RechargeStatus
{
    Pending,
    Completed,
    Failed
}