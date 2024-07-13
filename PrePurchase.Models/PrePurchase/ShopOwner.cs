using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;

public class ShopOwner
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
    [Required] public string ShopName { get; set; }

    [Required] [EmailAddress] public string Email { get; set; }

    [Required] [Phone] public string PhoneNumber { get; set; }

    [Required] public string Password { get; set; }

    public string QRCode { get; set; }
    public ICollection<Collection> Collections { get; set; }
}