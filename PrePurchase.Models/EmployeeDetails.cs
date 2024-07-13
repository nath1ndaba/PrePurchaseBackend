using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;
using PrePurchase.Models.Payments;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class EmployeeDetails
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
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

        public DateTime DateOfBirth { get; set; }
        [BsonRequired]
        public string Name { get; set; }

        [BsonRequired]
        public string Surname { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string TaxNumber { get; set; }
        public string IDNumber { get; set; }
        public DateTime DateOfEmployment { get; set; }
        public Address EmployeeAddress { get; set; }

        [BsonRequired]
        public string CellNumber { get; set; }

        [BsonRequired]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [BsonRequired]
        [JsonIgnore]
        public string Password { get; set; }
        [JsonIgnore]
        public string Pin { get; set; }

        public string DeviceInfo { get; set; }
        public BankAccountModel BankAccountInfo { get; set; }

        public void Deconstruct(out string name, out string surname)
        {
            name = Name;
            surname = Surname;
        }

        public void Deconstruct(out ObjectId id, out string name, out string surname)
        {
            id = Id;
            name = Name;
            surname = Surname;
        }

        public void Deconstruct(out ObjectId id, out string employeeId
            , out DateTime dateOfBirth, out string name, out string surname
            , out string nickName, out string cellNumber, out string email, out string taxNumber, out string idNumber, out DateTime dateOfEmployment, out DateTime timestamp, out Address address, out BankAccountModel bankAccountInfo)
        {
            id = Id;
            employeeId = EmployeeId;
            dateOfBirth = DateOfBirth;
            name = Name;
            surname = Surname;
            nickName = NickName;
            cellNumber = CellNumber;
            taxNumber = TaxNumber;
            email = Email;
            idNumber = IDNumber;
            address = EmployeeAddress;
            dateOfEmployment = DateOfEmployment;
            bankAccountInfo = BankAccountInfo;
            timestamp = Timestamp;
        }

        public static explicit operator EmployeeSummary(EmployeeDetails employeeDetails)
        {
            return new()
            {
                Id = employeeDetails.Id,
                EmployeeId = employeeDetails.EmployeeId,
                Name = employeeDetails.Name,
                Surname = employeeDetails.Surname
            };
        }

    }

    [BsonIgnoreExtraElements]
    public class EmployeeSummary
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        [BsonRequired]
        public ObjectId Id { get; set; }
        [BsonRequired]
        public string EmployeeId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
    }

}
