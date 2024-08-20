using global::PrePurchase.Models.Converters;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models.PrePurchase;
public class UserDto
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public string Id { get; set; }

    public DateTime CreatedDate { get; set; }

    [JsonConverter(typeof(ObjectIdConverter))]
    public string CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    [JsonConverter(typeof(ObjectIdConverter))]
    public string UpdatedBy { get; set; }

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

    public List<string> ShopId { get; set; }

    public Address Address { get; set; }
    public string NotificationToken { get; set; } //for firestore

}
