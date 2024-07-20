using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Actions.PrePurchase;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("api/v1")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Shop)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class LoginController : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<SucessfulLogin>), 200)]
        public async Task<Response> Login([FromBody] LoginModel model)
        {
            UserLoginResponse response = await _userLoginActions.UserLogin(model.Sanitize());
            JwtTokenModel jwt = await GetAuthTokens(response);

            SucessfulLogin sucessfulLogin = new()
            {
                UserLoginResponse = response,
                JwtTokenModel = jwt
            };

            return new Response<SucessfulLogin>(sucessfulLogin);
        }

        public LoginController(IUserLoginActions loginActions, IAuthContainerModel containerModel, IAuthService authService,
            IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _userLoginActions = loginActions;
        }

        private Task<JwtTokenModel> GetAuthTokens(UserLoginResponse loginResponse)
        {
            Claim[] claims =
            {
                new(ClaimTypes.Role, AuthRoles.Owner), new(ClaimTypes.Email, loginResponse.User.Email),
                new(ClaimTypes.NameIdentifier, loginResponse.User.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, loginResponse.User.Id.ToString())
            };

            return GetAuthTokens(claims);
        }

        private readonly IUserLoginActions _userLoginActions;
    }
}