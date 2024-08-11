using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.JWT;
using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("prepurchase")]
    [Route("[area]/[controller]")]
    [ApiController]

    public class PrePurchaseConvertCashToItemController : BaseController
    {
        [HttpPost("ConvertCashToItem/{userId}")]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> ConvertCashToItem([FromBody] CashToItemDto model, [FromRoute] string userId)
        {
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            Response response =
                await _cashToItemsActions.ConvertCashToItem(model, createdBy, userId);
            return response;
        }

        [HttpPut("UpdateCashToItem")]
        [ProducesResponseType(typeof(Response<CashToItemDto>), 200)]
        public async Task<Response> UpdateCashToItem([FromQuery] CashToItemDto amount, [FromRoute] string userId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            Response response =
                await _cashToItemsActions.UpdateCashToItem(amount, updatedBy, userId);
            return response;
        }

        [HttpGet("GetCashToItem/{rechargeId}")]
        [ProducesResponseType(typeof(Response<CashToItemDto>), 200)]
        public async Task<Response> GetCashToItem([FromRoute] string rechargeId, [FromQuery] string userId)
        {
            Response response =
                await _cashToItemsActions.GetCashToItem(rechargeId, userId);
            return response;
        }


        [HttpGet("GetTopNearbyShops")]
        [ProducesResponseType(typeof(Response<List<ShopDto>>), 200)]
        public async Task<Response> GetTopNearbyShops([FromQuery] ResidentLocation residentLocation, [FromQuery] int topN)
        {
            Response response =
                await _cashToItemsActions.GetTopNearbyShops(residentLocation, topN);
            return response;
        }

        [HttpGet("getRecharges/{userId}")]
        [ProducesResponseType(typeof(Response<List<CashToItemDto>>), 200)]
        public async Task<Response> GetCashToItems([FromRoute] string userId)
        {
            Response response =
                await _cashToItemsActions.GetCashToItems(userId);

            return response;
        }


        [HttpGet("GetCashToItems/{userId}")]
        [ProducesResponseType(typeof(Response<List<CashToItemDto>>), 200)]
        public async Task<Response> GetDashboardData([FromRoute] string userId)
        {
            Response response =
                await _cashToItemsActions.GetCashToItems(userId);

            return response;
        }

        public PrePurchaseConvertCashToItemController(
          IAuthContainerModel containerModel
          , IAuthService authService
          , IRepository<RefreshToken> refreshTokens
          , ICashToItemActions cashToItemActions)
          : base(containerModel, authService, refreshTokens)
        {
            _cashToItemsActions = cashToItemActions;
        }

        private readonly ICashToItemActions _cashToItemsActions;

        protected string UserId => User.FindFirstValue(PrePurchaseJwtConstants.UserId);
    }
}