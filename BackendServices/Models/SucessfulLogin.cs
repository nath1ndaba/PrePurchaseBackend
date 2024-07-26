using PrePurchase.Models.PrePurchase;

namespace BackendServices.Models
{
    public class SucessfulLogin
    {
        public JwtTokenModel Tokens { get; set; }
        public UserLoginResponse UserLoginResponse { get; set; }
    }
}
