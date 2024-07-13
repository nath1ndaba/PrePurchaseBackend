using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;
using PrePurchase.Models.Payments;

namespace PrePurchase.Models.StatementsModels
{
    public class Supplier
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); //The ID is required by the database for the Primary Key
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }

        [BsonRequired]
        public string CompanyName { get; set; }
        [BsonRequired]
        public string Address { get; set; }

        [BsonRequired]
        public string ContactPerson { get; set; }
        [BsonRequired]
        public string Email { get; set; }

        [BsonRequired]
        public string ContactTellNumber { get; set; }
        [BsonRequired]
        public string WhatsAppNumber { get; set; }
        public string PaymentMethod { get; set; }
        public double TotalAmount { get; set; }
        public BankAccountModel BankAccountInfo { get; set; }

    }
}
