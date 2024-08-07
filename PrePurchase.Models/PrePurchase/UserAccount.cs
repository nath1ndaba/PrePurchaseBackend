using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;

public class UserAccount
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

    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId UserId { get; set; } // Foreign key to the User

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal AmountBalance { get; set; }

    public List<ItemBalance> ItemsBalances { get; set; } // Account balance
}

public class ItemBalance
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Balance { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]

    public ObjectId ItemId { get; set; }
}

public class ItemBalancData
{
    public decimal Balance { get; set; }

    [JsonConverter(typeof(ObjectIdConverter))]

    public ObjectId ItemId { get; set; }


}


