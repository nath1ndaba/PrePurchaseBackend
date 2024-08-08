using BackendServices.Models.Inventory;
using BackendServices.Models.PrePurchase;
using System.Collections.Generic;

namespace PrePurchase.Models.PrePurchase
{
    public class UserLoginResponse
    {
        public UserDto User { get; set; }
        public UserAccountDto UserAccount { get; set; }
        public List<ShopDto> Shop { get; set; }

        public List<BackendServices.Models.Inventory.ProductDto> Products { get; set; }


    }
}
