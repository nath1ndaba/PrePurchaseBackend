using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendServices.Models
{
    public class SucessfulLogin
    {
        public JwtTokenModel JwtTokenModel { get; set; }
        public LoginResponse LoginResponse { get; set; }
    }
}
