using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class Times
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); //The ID is required by the database for the Primary Key

        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
    }
}
