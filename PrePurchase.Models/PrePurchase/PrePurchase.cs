using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;
using PrePurchase.Models.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PrePurchase.Models.PrePurchase;

public class PrePurchase
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
    [Required] public decimal Quantity { get; set; }

    [Required] public decimal TotalCost { get; set; }

    [Required] public DateTime PurchaseDate { get; set; }

    public int ResidentId { get; set; }

    public List<Product> Items { get; set; }

    [Required]
    public bool IsFullItem { get; set; }
}