using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrePurchase.Models
{
    [BsonNoId]
    [BsonIgnoreExtraElements]
    public record AddressDto
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId AddressBelongsToId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
