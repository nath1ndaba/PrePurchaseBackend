using System.Threading.Tasks;
using BackendServices;
using BackendServices.Actions.PrePurchase.AdminPortal;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PrePurchase.Models;

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("prepurchase")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Company)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class PrePurchaseAdminController : BaseController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> Register([FromBody] AdminRegisterModel model)
        {
            Response response =
                await _registerAdminActions.Register(model,
                    ObjectId.GenerateNewId()); //id not necessary at this stage/for now

            return response;
        }

        public PrePurchaseAdminController(IRegisterAdminActions registerAdminActions,
            IAuthContainerModel containerModel,
            IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService,
            refreshTokens)
        {
            _registerAdminActions = registerAdminActions;
        }

        private readonly IRegisterAdminActions _registerAdminActions;
    }
}