using global::PrePurchase.Models.Converters;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models;
using System;

namespace BackendServices.Models.PrePurchase;

public class ShopDto
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    public string Id { get; set; }
    public DateTime CreatedDate { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]
    public string CreatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]
    public string UpdatedBy { get; set; }
    public bool? DeletedIndicator { get; set; }

    [BsonRequired]
    public string Name { get; set; }
    public Address Address { get; set; }
    [BsonRequired]
    public string RegisterationNumber { get; set; }
    public string Email { get; set; }
    public string ContactNumber { get; set; }
    public string QRCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime? LicenseExpiryDate { get; set; }
}
