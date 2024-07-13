using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;

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
    [Required] public int Quantity { get; set; }

    [Required] public decimal TotalCost { get; set; }

    [Required] public DateTime PurchaseDate { get; set; }

    public int ResidentId { get; set; }
    [ForeignKey("ResidentId")] public Resident Resident { get; set; }

    public int ItemId { get; set; }
    [ForeignKey("ItemId")] public Item Item { get; set; }
}