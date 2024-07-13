using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public class CustomizationModel
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonRequired]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); //The ID is required by the database for the Primary Key
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }
        public string CustomAuth1 { get; set; }
        public string CustomText1 { get; set; }
        public string CustomAuth2 { get; set; }
        public string CustomText2 { get; set; }
        public string CustomAuth3 { get; set; }
        public string CustomText3 { get; set; }
        public string CustomAuth4 { get; set; }
        public string CustomText4 { get; set; }
        public string CustomAuth5 { get; set; }
        public string CustomText5 { get; set; }
        public string CustomAuth6 { get; set; }
        public string CustomText6 { get; set; }
        public string CustomAuth7 { get; set; }
        public string CustomText7 { get; set; }
        public string CustomAuth8 { get; set; }
        public string CustomText8 { get; set; }
    }
}
