using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class ProcessPayments
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }
        [BsonRequired]
        public string EmployeeId { get; set; }


        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonRequired]
        public ObjectId CompanyId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string NickName { get; set; }
        [BsonRequired]
        public string Position { get; set; }
        public string Department { get; set; }

        [BsonRequired]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow; //Stamp time processed

        public void UpdateUserDetails(EmployeeDetails employeeDetails)
        {
            EmployeeId = employeeDetails.EmployeeId;
            Name = employeeDetails.Name;
            Surname = employeeDetails.Surname;
            NickName = employeeDetails.NickName;
        } //users Details collected
    }
}
