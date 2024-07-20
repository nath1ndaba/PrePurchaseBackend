//using BackendServices;
//using BackendServices.Actions.Inventory;
//using BackendServices.Exceptions;
//using BackendServices.JWT;
//using BackendServices.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using ST.Models;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using PrePurchase.Models.Inventory;



//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace BackendServer.V1.Controllers
//{
//    [Produces("application/json")]
//    [Consumes("application/json")]
//    [Area("Inventory")]
//    [Route("[area]/[controller]")]
//    [Authorize(Policy = AuthPolicies.shop)]
//    [ProducesResponseType(typeof(Response), 400)]
//    [ProducesResponseType(typeof(Response), 500)]
//    [ApiController]
//    public class SupplierController : BaseController
//    {
//        private readonly ISuppliersActions actions;
//        public SupplierController(ISuppliersActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
//        {
//            this.actions = actions;
//        }

//        //[HttpPost]
//        //[ProducesResponseType(typeof(Response<Supplier>), 200)]
//        //public async Task<Response> AddSupplier([FromBody] Supplier model, [FromQuery] string shopid = null)
//        //{
//        //    RequireSupplierId(shopid);
//        //    string createdBy = User.FindFirstValue(ClaimTypes.Name);
//        //    string updatedBy = createdBy;
//        //    string role = GetRole();
//        //    Response response = await actions.AddSupplier(createdBy, updatedBy, model, role, shopid);

//        //    if (response is not Response<Supplier> correspondingResponse) return response;

//        //    return new Response<Supplier>(correspondingResponse.Data!);
//        //}

//        [HttpPut]
//        [ProducesResponseType(typeof(Response<Supplier>), 200)]
//        public async Task<Response> UpdateSupplier([FromBody] Supplier model, [FromQuery] string shopid = null)
//        {
//            RequireSupplierId(shopid);
//            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
//            string role = GetRole();

//            Response response = await actions.UpdateSupplier(updatedBy, model, role, shopid);

//            if (response is not Response<Supplier> correspondingResponse) return response;

//            return new Response<Supplier>(correspondingResponse.Data!);
//        }

//        [HttpGet]
//        [ProducesResponseType(typeof(Response<Supplier>), 200)]
//        public async Task<Response> GetSupplier([FromQuery] string shopid = null)
//        {
//            RequireSupplierId(shopid);
//            string role = GetRole();
//            return await actions.GetSupplier(Id, role, shopid);
//        }

//        [HttpGet("{id}")]
//        [ProducesResponseType(typeof(Response<List<Supplier>>), 200)]
//        public async Task<Response> GetSupplier([FromRoute] string id, [FromQuery] string shopid = null)
//        {
//            RequireSupplierId(shopid);
//            string role = GetRole();

//            return await actions.GetSupplier(id, role, shopid);
//        }

//        [HttpDelete("{id}")]
//        [ProducesResponseType(typeof(Response<List<Supplier>>), 200)]
//        public async Task<Response> SoftDeleteSupplier([FromRoute] string id, [FromQuery] string shopid = null)
//        {
//            RequireSupplierId(shopid);
//            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
//            string role = GetRole();

//            return await actions.SoftDeleteSupplier(updatedBy, id, role, shopid);
//        }

//        private string GetRole()
//        {
//            var accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

//            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

//        }

//#nullable enable
//        private string? GetId(string? memberId = null)
//        {
//            var role = GetRole();

//            if (role == AuthRoles.Owner)
//                return Id;

//            return memberId;
//        }

//        private void RequireSupplierId(string memberId)
//        {
//            var role = GetRole();
//            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(memberId))
//                throw new HttpResponseException(new Response(error: "Supplier id is not specified!")); ;

//        }

//#nullable disable

//    }
//}
