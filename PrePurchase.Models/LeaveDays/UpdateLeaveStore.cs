using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace PrePurchase.Models.LeaveDays
{
    [BsonIgnoreExtraElements]
    public class UpdateLeaveStore
    {
        [Required]
        public string StoreId { get; set; }
        public decimal AnnualLeaveDays { get; set; }
        public decimal SickLeaveDays { get; set; }
        public decimal FamilyLeaveDays { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
