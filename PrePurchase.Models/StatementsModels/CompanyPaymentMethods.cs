using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.StatementsModels
{
    public class CompanyPaymentMethods
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); //The ID is required by the database for the Primary Key

        [BsonRequired]
        public string PaymentMethod { get; set; }
    }
}
