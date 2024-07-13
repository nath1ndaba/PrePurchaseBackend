using BackendServices;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly IAuthService authService;
        protected readonly IAuthContainerModel containerModel;
        protected readonly IRepository<RefreshToken> refreshTokens;

        protected string Id => User.FindFirstValue(ClaimTypes.Name);

        public BaseController(IAuthContainerModel containerModel
            , IAuthService authService
            , IRepository<RefreshToken> refreshTokens)
        {
            this.authService = authService;
            this.containerModel = containerModel;
            this.refreshTokens = refreshTokens;
        }

        protected async Task<JwtTokenModel> GetAuthTokens(IEnumerable<Claim> claims)
        {
            var accessTokenId = RefreshToken.GenerateNewId();
            Claim _accesTokenClaim = new(JwtRegisteredClaimNames.Jti, accessTokenId);
            Claim[] _claims = claims.Append(_accesTokenClaim).ToArray();

            var accessTokenModel = containerModel.WithClaimes(_claims);

            var refreshTokenModel = containerModel
                                    .WithClaimes(new Claim[] {
                                        new(ClaimTypes.Role, AuthRoles.RefreshToken)
                                        , _accesTokenClaim
                                    })
                                    .WithExpiresMinutes(PrePurchaseConfig.JWT_REFRESH_TOKEN_EXPIRES_IN);

            var refreshToken = authService.GenerateJwtToken(refreshTokenModel);

            var accessToken = authService.GenerateJwtToken(accessTokenModel);

            RefreshToken model = RefreshToken.WithId(accessTokenId);
            model = model with
            {
                ExpiryDate = DateTime.UnixEpoch.AddSeconds(refreshToken.ExpiresAt),
                TimeStamp = DateTime.UtcNow,
                Token = refreshToken.Token,
                Invalidated = false,
            };

            await refreshTokens.Insert(model);

            return new(refreshToken, accessToken);
        }
    }
}
