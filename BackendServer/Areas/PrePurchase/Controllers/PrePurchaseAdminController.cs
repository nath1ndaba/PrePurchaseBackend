using System.Security.Claims;
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
                await _registerAdminActions.RegisterAdmin(model,
                    ObjectId.GenerateNewId()); //id not necessary at this stage/for now

            return response;
        }

        [HttpGet("getadmins")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> GetAdmins()
        {
            Response response =
                await _registerAdminActions.GetAdmins();

            return response;
        }

        [HttpGet("getadmin/{adminId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> GetAdmins([FromRoute] string adminId)
        {
            Response response =
                await _registerAdminActions.GetAdmin(adminId);

            return response;
        }

        [HttpPatch("archiveadmin/{adminId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> ArchiveAdmin([FromRoute] string adminId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            Response response = await _registerAdminActions.ArchiveAdmin(adminId, updatedBy);
            return response;
        }

        [HttpPatch("restoreadmin/{adminId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> RestoreAdmin([FromRoute] string adminId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            Response response =
                await _registerAdminActions.ArchiveAdmin(adminId, updatedBy);

            return response;
        }

        public PrePurchaseAdminController(IAdminActions registerAdminActions,
            IAuthContainerModel containerModel,
            IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService,
            refreshTokens)
        {
            _registerAdminActions = registerAdminActions;
        }

        private readonly IAdminActions _registerAdminActions;
    }
}