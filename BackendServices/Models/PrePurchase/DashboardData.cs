using BackendServices.Models.Inventory;
using PrePurchase.Models.PrePurchase;
using System.Collections.Generic;

namespace BackendServices.Models.PrePurchase
{
    public class DashboardData
    {

        public string UserId { get; set; }

        public decimal AmountBalance { get; set; }

        public List<ItemBalance> ItemBalances { get; set; }
    }
}
