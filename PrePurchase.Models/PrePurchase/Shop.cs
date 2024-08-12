using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;

using System.ComponentModel.DataAnnotations;

public class Shop
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId? CreatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId? UpdatedBy { get; set; }
    public bool? DeletedIndicator { get; set; }

    [BsonRequired]
    public string Name { get; set; }
    [BsonRequired]
    public string RegisterationNumber { get; set; }
    public string Email { get; set; }
    public string ContactNumber { get; set; }
    public string QRCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime? LicenseExpiryDate { get; set; }
}