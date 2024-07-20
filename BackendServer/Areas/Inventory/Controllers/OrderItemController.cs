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
    [Authorize(Policy = AuthPolicies.Shop)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class OrderItemController : BaseController
    {
        private readonly IOrderItemsActions actions;
        public OrderItemController(IOrderItemsActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<OrderItem>), 200)]
        public async Task<Response> AddOrderItem([FromBody] OrderItem model, [FromQuery] string companyid = null)
        {
            RequireOrderItemId(companyid);
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            string role = GetOrderItem();
            Response response = await actions.AddOrderItem(createdBy, updatedBy, model, role, companyid);

            if (response is not Response<OrderItem> correspondingResponse) return response;

            return new Response<OrderItem>(correspondingResponse.Data!);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Response<OrderItem>), 200)]
        public async Task<Response> UpdateOrderItem([FromBody] OrderItem model, [FromQuery] string companyid = null)
        {
            RequireOrderItemId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetOrderItem();

            Response response = await actions.UpdateOrderItem(updatedBy, model, role, companyid);

            if (response is not Response<OrderItem> correspondingResponse) return response;

            return new Response<OrderItem>(correspondingResponse.Data!);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<OrderItem>), 200)]
        public async Task<Response> GetOrderItem([FromQuery] string companyid = null)
        {
            RequireOrderItemId(companyid);
            string role = GetOrderItem();
            return await actions.GetOrderItem(Id, role, companyid);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<List<OrderItem>>), 200)]
        public async Task<Response> GetOrderItem([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireOrderItemId(companyid);
            string role = GetOrderItem();

            return await actions.GetOrderItem(id, role, companyid);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<List<OrderItem>>), 200)]
        public async Task<Response> SoftDeleteOrderItem([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireOrderItemId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetOrderItem();

            return await actions.SoftDeleteOrderItem(updatedBy, id, role, companyid);
        }

        private string GetOrderItem()
        {
            var accpetedOrderItems = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedOrderItems.Contains(x.Value)).Value;

        }

#nullable enable
        private string? GetId(string? memberId = null)
        {
            var role = GetOrderItem();

            if (role == AuthRoles.Owner)
                return Id;

            return memberId;
        }

        private void RequireOrderItemId(string memberId)
        {
            var role = GetOrderItem();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(memberId))
                throw new HttpResponseException(new Response(error: "OrderItem id is not specified!")); ;

        }

#nullable disable

    }
}
