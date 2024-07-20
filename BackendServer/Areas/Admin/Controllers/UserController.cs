using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrePurchase.Models;
using System.Linq;
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
    public class UserController : BaseController
    {
        [HttpPost("RegisterUser")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<DiscontinuedUser>), 201)]
        public async Task<Response> RegisterUser([FromBody] DiscontinuedUser model)
        {
            return await _userActions.RegisterUser(model);
        }


        public UserController(IDiscontinuedUserActions userActions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _userActions = userActions;

        }

        private string GetRole()
        {
            string[] accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }

#nullable enable
        private string? GetId(string? companyId = null)
        {
            var role = GetRole();

            if (role == AuthRoles.Owner)
                return Id;

            return companyId;
        }

#nullable disable

        private readonly IDiscontinuedUserActions _userActions;
    }
}
