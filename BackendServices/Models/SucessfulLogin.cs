using PrePurchase.Models.PrePurchase;

namespace BackendServices.Models
{
    public class SucessfulLogin
    {
        public JwtTokenModel JwtTokenModel { get; set; }
        public UserLoginResponse UserLoginResponse { get; set; }
    }
}
