using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;

public class Collection
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

    [Required]
    public DateTime CollectionDate { get; set; }

    [Required]
    public string ItemType { get; set; }

    [Required]
    public bool IsFullItem { get; set; }

    public int ResidentId { get; set; }
    [ForeignKey("ResidentId")]
    public Resident Resident { get; set; }

    public int SpazaShopOwnerId { get; set; }
    [ForeignKey("SpazaShopOwnerId")]
    public ShopOwner SpazaShopOwner { get; set; }

    public int ItemId { get; set; }
    [ForeignKey("ItemId")]
    public Item Item { get; set; }
}