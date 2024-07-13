using BackendServices;
using BackendServices.Actions.Inventory;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using PrePurchase.Models.Inventory;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("Inventory")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Company)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class StockCountController : BaseController
    {
        private readonly IStockCountsActions actions;
        public StockCountController(IStockCountsActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<StockCount>), 200)]
        public async Task<Response> AddStockCount([FromBody] StockCount model, [FromQuery] string companyid = null)
        {
            RequireStockCountId(companyid);
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            string role = GetRole();
            Response response = await actions.AddStockCount(createdBy, updatedBy, model, role, companyid);

            if (response is not Response<StockCount> correspondingResponse) return response;

            return new Response<StockCount>(correspondingResponse.Data!);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Response<StockCount>), 200)]
        public async Task<Response> UpdateStockCount([FromBody] StockCount model, [FromQuery] string companyid = null)
        {
            RequireStockCountId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            Response response = await actions.UpdateStockCount(updatedBy, model, role, companyid);

            if (response is not Response<StockCount> correspondingResponse) return response;

            return new Response<StockCount>(correspondingResponse.Data!);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<StockCount>), 200)]
        public async Task<Response> GetStockCount([FromQuery] string companyid = null)
        {
            RequireStockCountId(companyid);
            string role = GetRole();
            return await actions.GetStockCount(Id, role, companyid);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<List<StockCount>>), 200)]
        public async Task<Response> GetStockCount([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireStockCountId(companyid);
            string role = GetRole();

            return await actions.GetStockCount(id, role, companyid);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<List<StockCount>>), 200)]
        public async Task<Response> SoftDeleteStockCount([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireStockCountId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            return await actions.SoftDeleteStockCount(updatedBy, id, role, companyid);
        }

        private string GetRole()
        {
            var accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }

#nullable enable
        private string? GetId(string? memberId = null)
        {
            var role = GetRole();

            if (role == AuthRoles.Owner)
                return Id;

            return memberId;
        }

        private void RequireStockCountId(string memberId)
        {
            var role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(memberId))
                throw new HttpResponseException(new Response(error: "StockCount id is not specified!")); ;

        }

#nullable disable

    }
}
