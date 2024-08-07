using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;

public class CashToItem
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
    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId ItemId { get; set; }
    public string ItemName { get; set; }
    public byte[] ItemImage { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal NumberOfItemsPurchased { get; set; }
    public decimal AmountSpentOnItem { get; set; }
    public decimal PreviousPriceToPurchaseItem { get; set; } //incase if prices increase on inventory NB: always update this with price from Inventory when a purchase takes place
}