using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("api/v1")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Company)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class LoginController : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<SucessfulLogin>), 200)]
        public async Task<Response> Login([FromBody] AdminLoginModel model)
        {
            LoginResponse response = await _loginActions.Login(model.Sanitize());
            JwtTokenModel jwt = await GetAuthTokens(response);

            SucessfulLogin sucessfulLogin = new()
            {
                LoginResponse = response,
                JwtTokenModel = jwt
            };

            return new Response<SucessfulLogin>(sucessfulLogin);
        }

        public LoginController(ILoginActions loginActions, IAuthContainerModel containerModel, IAuthService authService,
            IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _loginActions = loginActions;
        }

        private Task<JwtTokenModel> GetAuthTokens(LoginResponse loginResponse)
        {
            Claim[] claims =
            {
                new(ClaimTypes.Role, AuthRoles.Owner), new(ClaimTypes.Email, loginResponse.LoggedUserEmailAddress),
                new(ClaimTypes.NameIdentifier, loginResponse.Id),
                new(JwtRegisteredClaimNames.UniqueName, loginResponse.Id)
            };

            return GetAuthTokens(claims);
        }

        private readonly ILoginActions _loginActions;
    }
}