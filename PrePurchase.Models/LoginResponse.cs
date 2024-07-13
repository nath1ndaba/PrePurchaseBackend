using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class LoginResponse
    {
        [BsonRequired]
        public string? Id { get; set; }
        [BsonRequired]
        public string? Name { get; set; }
        [BsonRequired]
        public string? SurName { get; set; }
        [BsonRequired]
        public string? NickName { get; set; }
        [BsonRequired]
        public string? EmployeeId { get; set; }
        [BsonRequired]
        public string? LoggedUserEmailAddress { get; set; }
        [BsonRequired]
        public IsSuperAdmin IsSuperAdmin { get; set; }
        public List<Company> Companies { get; set; }
        public List<Project> Projects { get; set; }
    }
}
