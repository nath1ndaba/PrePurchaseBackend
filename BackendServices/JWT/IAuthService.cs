using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Security.Claims;

namespace BackendServices.JWT
{
    public interface IAuthService
    {
        bool IsTokenValid(string token);
        string GenerateToken(IAuthContainerModel model);
        JwtToken GenerateJwtToken(IAuthContainerModel model);
#nullable enable
        IEnumerable<Claim>? GetTokenClaims(string token, string? secretKey = default);
        ClaimsPrincipal GetClaimsPrincipal(string token, string? secretKey = default);

        IEnumerable<Claim>? PeekClaims(string token);

#nullable restore

        internal TokenValidationParameters GetTokenValidationParameters(string secretKey = default);
    }
}
