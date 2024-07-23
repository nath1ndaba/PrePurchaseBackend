using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace PrePurchase.Models.PrePurchase;

public class RechargeDto
{
    [BsonRequired]
    public string Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; }

    public bool DeletedIndicator { get; set; }
    public string UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
    public DateTime RechargeDate { get; set; }
    public RechargeStatus Status { get; set; } // "Pending", "Completed", "Failed"
    public string PaymentMethod { get; set; } // "CreditCard", "DebitCard", "BankTransfer", etc.
}