using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.StatementsModels
{
    public class SupplierInvoices
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); //The ID is required by the database for the Primary Key
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId SupplierId { get; set; }
        [BsonRequired]
        public DateTime RecievedDate { get; set; } = DateTime.UtcNow;

        [BsonRequired]
        public String InvoiceNumber { get; set; }
        [BsonRequired]
        public double TotalAmount { get; set; }



    }
}
