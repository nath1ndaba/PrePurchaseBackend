using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class Amounts
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); //The ID is required by the database for the Primary Key
        //fetching user's details

        public long TotalAmount { get; set; }
        public long PublicHolidays { get; set; }
        public long Sundays { get; set; }
        public long NormalDays { get; set; }
        public long Bonus { get; set; }

    }
}
