using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.PrePurchase;

using System.ComponentModel.DataAnnotations;

public class Admin
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
    [Required] public string Name { get; set; }
    [Required] public string Surname { get; set; }

    [Required][EmailAddress] public string Email { get; set; }

    [Required][Phone] public string PhoneNumber { get; set; }

    [Required] public string Password { get; set; }
}