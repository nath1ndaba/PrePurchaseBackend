using BackendServices.Models.PrePurchase;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrePurchase.Models.PrePurchase
{
    public class UserLoginResponse
    {
        public UserDto User { get; set; }

        public List<ShopDto> Shop { get; set; }

    }
}
