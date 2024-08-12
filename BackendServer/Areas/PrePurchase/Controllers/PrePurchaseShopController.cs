using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.JWT;
using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("prepurchase")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Shop)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class PrePurchaseShopController : BaseController
    {
        [HttpPost("register/{adminId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> Register([FromBody] ShopDto model, [FromRoute] string adminId)
        {
            Response response =
                await _registerShopActions.RegisterShop(model, adminId);
            return response;
        }

        [HttpGet("getShops")]
        //[ProducesResponseType(typeof(Response<List<ShopDto>>), 200)]
        public async Task<Response> GetShops()
        {
            Response response =
                await _registerShopActions.GetShops();

            return response;
        }

        [HttpGet("getShop/{ShopId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> GetShops([FromRoute] string ShopId)
        {
            Response response =
                await _registerShopActions.GetShop(ShopId);

            return response;
        }

        [HttpPatch("archiveShop/{ShopId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> ArchiveShop([FromRoute] string ShopId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            Response response = await _registerShopActions.ArchiveShop(ShopId, updatedBy);
            return response;
        }

        [HttpPatch("restoreShop/{ShopId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> RestoreShop([FromRoute] string ShopId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            Response response =
                await _registerShopActions.ArchiveShop(ShopId, updatedBy);

            return response;
        }

        public PrePurchaseShopController(IShopActions registerShopActions,
            IAuthContainerModel containerModel,
            IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService,
            refreshTokens)
        {
            _registerShopActions = registerShopActions;
        }

        private readonly IShopActions _registerShopActions;
    }
}