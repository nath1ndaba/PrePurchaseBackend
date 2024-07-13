using BackendServices;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
#nullable enable
    [Area("api/v1")]
    [Route("[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = AuthRoles.RefreshToken)]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IAuthContainerModel authContainerModel;
        private readonly IRepository<RefreshToken> refreshTokens;

        public AuthController(IAuthService authService, IAuthContainerModel authContainerModel, IRepository<RefreshToken> refreshTokens)
        {
            this.authService = authService;
            this.authContainerModel = authContainerModel;
            this.refreshTokens = refreshTokens;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        [ProducesResponseType(typeof(Response), 400)]
        public async Task<Response> Refresh([Required] string access_token)
        {
            // refresh the token
            // - access token 
            // - get stored refresh token
            // return new access token

            var accessTokenClaims = authService.PeekClaims(access_token);
            if (accessTokenClaims is null)
                throw new HttpResponseException($@"""{access_token}"" is not a valid access_token!");

            var id = accessTokenClaims.First(x => x.Type == JwtRegisteredClaimNames.Jti);

            if (id is null || id.Value != User.FindFirstValue(JwtRegisteredClaimNames.Jti))
                throw new HttpResponseException(error: $@"""{access_token}"" is not a valid access_token!");

            var refreshToken = await refreshTokens.FindById(id.Value);

            if (refreshToken is null || refreshToken.Invalidated)
                throw new HttpResponseException(error: $@"""{access_token}"" is not a valid access_token!");

            string expString = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

            if (!long.TryParse(expString, out long exp))
                throw new HttpResponseException(error: $@"""{access_token}"" is not a valid access_token!");


            JwtToken _refreshToken = new(refreshToken.Token, exp);
            var accessTokenModel = authContainerModel.WithClaimes(accessTokenClaims);
            var accessToken = authService.GenerateJwtToken(accessTokenModel);

            return new Response<JwtTokenModel>(new(_refreshToken, accessToken));

        }
    }

#nullable restore
}
