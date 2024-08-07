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

    public class PrePurchaseRechargeAccountController : BaseController
    {
        [HttpPost("recharge/{userId}")]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> RechargeAccount([FromBody] RechargeDto model, [FromRoute] string userId)
        {
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            Response response =
                await _registerRechargeAccountActions.RechargeAccount(model, createdBy, userId);
            return response;
        }

        [HttpPut("UpdateUserAccountBalance")]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> UpdateUserAccountBalance([FromQuery] decimal amount, [FromRoute] string userId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            Response response =
                await _registerRechargeAccountActions.UpdateUserAccountBalance(amount, updatedBy, userId);
            return response;
        }

        [HttpGet("getRecharge/{rechargeId}")]
        [ProducesResponseType(typeof(Response<RechargeDto>), 200)]
        public async Task<Response> GetRechargeAccount([FromRoute] string rechargeId, [FromQuery] string userId)
        {
            Response response =
                await _registerRechargeAccountActions.GetRecharge(rechargeId, userId);
            return response;
        }

        [HttpGet("getRecharges/{userId}")]
        [ProducesResponseType(typeof(Response<List<RechargeDto>>), 200)]
        public async Task<Response> GetRechargeAccounts([FromRoute] string userId)
        {
            Response response =
                await _registerRechargeAccountActions.GetRecharges(userId);

            return response;
        }

        [HttpGet("GetUserAccountBalance/{userId}")]
        [ProducesResponseType(typeof(Response<UserAccountDto>), 200)]
        public async Task<Response> GetUserAccountBalance([FromRoute] string userId)
        {
            Response response =
                await _registerRechargeAccountActions.GetUserAccountBalance(userId);

            return response;
        }

        [HttpGet("GetDashboardData/{userId}")]
        [ProducesResponseType(typeof(Response<DashboardData>), 200)]
        public async Task<Response> GetDashboardData([FromRoute] string userId)
        {
            Response response =
                await _registerRechargeAccountActions.GetDashboardData(userId);

            return response;
        }

        public PrePurchaseRechargeAccountController(
          IAuthContainerModel containerModel
          , IAuthService authService
          , IRepository<RefreshToken> refreshTokens
          , IRechargeAccountActions registerRechargeAccountActions)
          : base(containerModel, authService, refreshTokens)
        {
            _registerRechargeAccountActions = registerRechargeAccountActions;
        }

        private readonly IRechargeAccountActions _registerRechargeAccountActions;

        protected string UserId => User.FindFirstValue(PrePurchaseJwtConstants.UserId);
    }
}