using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models.PrePurchase;
public class CashToItemDto
{
    [BsonRequired]
    public string Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; }

    public bool DeletedIndicator { get; set; }

    public string UserId { get; set; }

    public string ItemId { get; set; }

    public string ItemName { get; set; }

    public byte[] ItemImage { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Number of items purchased must be greater than zero.")]
    public decimal NumberOfItemsPurchased { get; set; }

    public decimal AmountSpentOnItem { get; set; }

    public decimal PreviousPriceToPurchaseItem { get; set; }
}