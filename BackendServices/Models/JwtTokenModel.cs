using BackendServices.JWT;

namespace BackendServices.Models
{
    public record JwtTokenModel (JwtToken RefreshToken, JwtToken AccessToken);
}
