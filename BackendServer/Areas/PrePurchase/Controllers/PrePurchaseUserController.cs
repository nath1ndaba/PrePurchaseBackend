using System.Security.Claims;
using System.Threading.Tasks;
using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.JWT;
using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;

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
    public class PrePurchaseUserController : BaseController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> Register([FromBody] UserDto model)
        {
            Response response =
                await _registerUserActions.RegisterUser(model, ObjectId.GenerateNewId().ToString()); //TODO: find a secure way of registering a user
            return response;
        }

        [HttpGet("getUsers")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> GetUsers()
        {
            Response response =
                await _registerUserActions.GetUsers();

            return response;
        }

        [HttpGet("getUser/{UserId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> GetUsers([FromRoute] string UserId)
        {
            Response response =
                await _registerUserActions.GetUser(UserId);

            return response;
        }

        [HttpPatch("archiveUser/{UserId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> ArchiveUser([FromRoute] string UserId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            Response response = await _registerUserActions.ArchiveUser(UserId, updatedBy);
            return response;
        }

        [HttpPatch("restoreUser/{UserId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> RestoreUser([FromRoute] string UserId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            Response response =
                await _registerUserActions.ArchiveUser(UserId, updatedBy);

            return response;
        }

        public PrePurchaseUserController(IUserActions registerUserActions,
            IAuthContainerModel containerModel,
            IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService,
            refreshTokens)
        {
            _registerUserActions = registerUserActions;
        }

        private readonly IUserActions _registerUserActions;
    }
}