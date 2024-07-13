using System;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.Payments
{
    public class TransactionModel
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; }

        public decimal Amount {get;set;}
        public decimal Fee {get;set;}
        public Currency Currency {get;set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonRequired]
        public ObjectId CompanyId {get;set;}
        public string TransactionId {get;set;}
        public string TransactionType {get;set;}
        public PaymentMethod PaymentMethod {get;set;}
        
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime TransactionDate {get;set;}
        public TransactionModel(){}
        public TransactionModel(string transactionId,decimal amount,
            Currency currency, string transactionType, 
            PaymentMethod paymentMethod, DateTime transactionDate, decimal fee = 0)
        {
                Id = ObjectId.GenerateNewId();//The ID is required by the database for the Primary Key
                TransactionId = transactionId;
                Amount = amount;
                Fee = fee;
                Currency = currency;
                TransactionType = transactionType;
                PaymentMethod = paymentMethod;
                TransactionDate = transactionDate;
        }
    }
}