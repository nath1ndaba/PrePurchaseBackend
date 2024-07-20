using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;
public class User
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
    public string Name { get; set; }
    [Required]
    public string SurName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserName { get; set; }

    public string PhoneNumber { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }


    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Required]
    public UserRole Role { get; set; }

    public List<ObjectId> ShopId { get; set; }

}

public enum UserRole
{
    Resident,
    Seller,
    ShopOwner
}
