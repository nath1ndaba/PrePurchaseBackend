using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public class AmendClocks
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public RateType RateType { get; set; }
    }
}
