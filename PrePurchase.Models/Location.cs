using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    /// <summary>
    /// Value Type containing the position data (Latitude and Longitude) information
    /// </summary>

    [BsonNoId]
    [BsonIgnoreExtraElements]
    public record Location
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string SiteName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}
