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
    public class PurchaseOrderController : BaseController
    {
        private readonly IPurchaseOrdersActions actions;
        public PurchaseOrderController(IPurchaseOrdersActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<PurchaseOrder>), 200)]
        public async Task<Response> AddPurchaseOrder([FromBody] PurchaseOrder model, [FromQuery] string companyid = null)
        {
            RequirePurchaseOrderId(companyid);
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            string role = GetRole();
            Response response = await actions.AddPurchaseOrder(createdBy, updatedBy, model, role, companyid);

            if (response is not Response<PurchaseOrder> correspondingResponse) return response;

            return new Response<PurchaseOrder>(correspondingResponse.Data!);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Response<PurchaseOrder>), 200)]
        public async Task<Response> UpdatePurchaseOrder([FromBody] PurchaseOrder model, [FromQuery] string companyid = null)
        {
            RequirePurchaseOrderId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            Response response = await actions.UpdatePurchaseOrder(updatedBy, model, role, companyid);

            if (response is not Response<PurchaseOrder> correspondingResponse) return response;

            return new Response<PurchaseOrder>(correspondingResponse.Data!);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<PurchaseOrder>), 200)]
        public async Task<Response> GetPurchaseOrder([FromQuery] string companyid = null)
        {
            RequirePurchaseOrderId(companyid);
            string role = GetRole();
            return await actions.GetPurchaseOrder(Id, role, companyid);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<List<PurchaseOrder>>), 200)]
        public async Task<Response> GetPurchaseOrder([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequirePurchaseOrderId(companyid);
            string role = GetRole();

            return await actions.GetPurchaseOrder(id, role, companyid);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<List<PurchaseOrder>>), 200)]
        public async Task<Response> SoftDeletePurchaseOrder([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequirePurchaseOrderId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            return await actions.SoftDeletePurchaseOrder(updatedBy, id, role, companyid);
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

        private void RequirePurchaseOrderId(string memberId)
        {
            var role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(memberId))
                throw new HttpResponseException(new Response(error: "PurchaseOrder id is not specified!")); ;

        }

#nullable disable

    }
}
