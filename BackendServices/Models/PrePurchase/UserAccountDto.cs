using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace BackendServices.Models.PrePurchase
{
    public class UserAccountDto
    {
        [BsonId]
        [BsonRequired]
        public string Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public bool DeletedIndicator { get; set; }

        public string UserId { get; set; }

        public decimal AmountBalance { get; set; }

        public List<ItemBalanceDto> ItemsBalances { get; set; }
    }

    public class ItemBalanceDto
    {
        public decimal Balance { get; set; }

        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public byte[] ItemImage { get; set; }
    }
}
