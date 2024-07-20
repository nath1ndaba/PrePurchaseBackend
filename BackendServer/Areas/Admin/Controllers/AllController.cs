using BackendServices;
using BackendServices.Actions;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.LeaveDays;
using PrePurchase.Models.StatementsModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("api/v1")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Shop)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class AllController : BaseController
    {
        private readonly IAllActions actions;
        public string CurrentUserEmail { get; set; }

        public AllController(IAllActions actions
            , IAuthContainerModel containerModel
            , IAuthService authService
            , IRepository<RefreshToken> refreshTokens)
            : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }


        [HttpGet("GetDeatiledAds")]
        [ProducesResponseType(typeof(Response<List<DetailedAd>>), 200)]
        public async Task<Response> GetDeatiledAds([FromQuery][Required] string? companyId = null)
        {
            var role = GetRole();
            return await actions.GetDeatiledAds(role, companyId);
        }

        [HttpPost("uploadAd")]
        [ProducesResponseType(typeof(Response<DetailedAd>), 201)]
        public async Task<IActionResult> UploadAds([FromBody][Required] DetailedAd model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await actions.UploadAds(model, createdBy, updatedBy, role, companyId);

            return StatusCode((int)response.StatusCode, response);

        }


        //[HttpPost("changePassword")]
        //[Authorize(Roles = AuthRoles.Owner)]
        //[ProducesResponseType(typeof(Response), 200)]
        //public async Task<Response> ChangePassword([FromBody] string userId , ChangePasswordModel model)
        //{
        //    return await actions.ChangePassword(Id,userId,  model);
        //}




        [HttpPut("rosta")]
        [ProducesResponseType(typeof(Response), 201)]
        public async Task<Response> AddEmployeeToRosta(AddEmployeeToRostaModel model, [FromQuery][Required] string? companyId = null)
        {
            var role = GetRole();
            return await actions.AddEmployeeToRosta(model, role, companyId);
        }


        [HttpDelete("rosta/{taskId}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> RemoveEmployeeFromRosta([FromRoute][Required] string taskId, [FromQuery] RemoveEmployeeFromRostaModel model, [FromQuery][Required] string? companyId = null)
        {
            var role = GetRole();

            model = model.WithTaskId(taskId);
            return await actions.RemoveEmployeeFromRosta(model, role, companyId);
        }

        //[HttpGet("rosta")]
        //[ProducesResponseType(typeof(Response<List<DetailedCompanyEmployee>>), 200)]
        //public async Task<Response> GetRostaForDepartment([FromQuery][Required] string department, [FromQuery] string? companyId = null)
        //{
        //    return await GetEmployeesByDepartment(department, companyId);
        //}

        [HttpPut("rostas")]
        [ProducesResponseType(typeof(Response), 201)]
        public async Task<Response> AddEmployeesToRosta(List<AddEmployeeToRostaModel> model, [FromQuery][Required] string? companyId = null)
        {
            RequireCompanyId(companyId);
            companyId = GetId(companyId);

            var role = GetRole();

            return await actions.AddEmployeesToRosta(model, role, companyId);
        }



        [HttpGet("leaves")]
        [ProducesResponseType(typeof(Response<List<Leave>>), 200)]
        public async Task<Response> GetLeaves([FromQuery] QueryLeaveModel model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            model = model with { CompanyId = companyId };

            Response response = await actions.GetLeaves(model, role);
            return response;
        }

        [HttpPatch("leave/{id}")]
        [ProducesResponseType(typeof(Response<Loan>), 200)]
        public async Task<Response> UpdateLeave([FromRoute][Required] string id, [Required] QueryLeaveModel model, [FromQuery][Required] string? companyId = null)
        {
            var role = GetRole();

            return await actions.UpdateLeave(id, model, role, companyId);
        }


        [HttpGet("GetLeaveStore")]
        [ProducesResponseType(typeof(Response<IEnumerable<LeaveGet>>), 200)]
        public async Task<Response> GetLeaveStore([FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            return await actions.GetLeaveStore(role, companyId);
        }
        [HttpPatch("UpdateLeaveStore")]
        [ProducesResponseType(typeof(Response<UpdateLeaveStore>), 200)]
        public async Task<Response> UpdateLeaveStore(List<UpdateLeaveStore> model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            return await actions.UpdateLeaveStore(model, role, companyId);
        }

        [HttpPost("AddCustomization")]
        [ProducesResponseType(typeof(Response<Customization>), 201)]
        public async Task<IActionResult> Customization([FromBody][Required] Customization model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            Response response = await actions.Customization(role, model, companyId);

            return StatusCode((int)response.StatusCode, response);

        }

        [HttpGet("GetCustomization")]
        [ProducesResponseType(typeof(Response<Customization>), 200)]
        public async Task<Response> GetCustomization([FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();

            return await actions.GetCustomization(role, companyId);
        }


        [HttpPost("AddSupplier")]
        [ProducesResponseType(typeof(Response<Supplier>), 201)]
        public async Task<IActionResult> AddSupplier([FromBody][Required] Supplier model, [FromQuery][Required] string? companyId = null)
        {

            string role = GetRole();
            Response response = await actions.AddSupplier(role, model, companyId);

            return StatusCode((int)response.StatusCode, response);

        }

        [HttpGet("GetSuppliers")]
        [ProducesResponseType(typeof(Response<Supplier>), 200)]
        public async Task<Response> GetSuppliers([FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();

            return await actions.GetSuppliers(role, companyId);
        }

        [HttpGet("SuppliersByPaymentMethod")]
        [ProducesResponseType(typeof(Response<List<Supplier>>), 200)]
        public async Task<Response> GetSuppliersByPaymentMethod([FromQuery][Required] string paymentMethod, [FromQuery] string? companyId = null)
        {
            string role = GetRole();

            return await actions.GetSuppliersByPaymentMethod(paymentMethod, role, companyId);
        }


        [HttpPut("SuppliersPaymentMethod")]
        [ProducesResponseType(typeof(Response<string>), 201)]
        public async Task<Response> AddPaymentMethod([FromBody][Required] string payment, [FromQuery] string? companyId = null)
        {
            string role = GetRole();
            return await actions.AddPaymentMethod(payment, role, companyId);

        }

        [HttpDelete("paymentMethod/{paymentMethod}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> RemovePaymentMethod([FromRoute][Required] string payment, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();

            return await actions.RemovePaymentMethod(payment, role, companyId);

        }


        [HttpPost("AddSupplierInvoice")]
        [ProducesResponseType(typeof(Response<SupplierInvoices>), 201)]
        public async Task<IActionResult> AddSupplierInvoice([FromBody][Required] SupplierInvoices model, [FromQuery][Required] string? companyId = null)
        {
            var role = GetRole();
            var response = await actions.AddSupplierInvoices(role, model, companyId);

            return StatusCode((int)response.StatusCode, response);

        }

        [HttpGet("GetSupplierInvoices/{supplierId}")]
        [ProducesResponseType(typeof(Response<SupplierInvoices>), 200)]
        public async Task<Response> GetSupplierInvoices([FromQuery][Required] string supplierId = null, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            return await actions.GetSuppliersInvoices(role, companyId, supplierId);
        }
        //Claim[] claims = {
        //    new(ClaimTypes.Role, AuthRoles.Owner)
        //    , new (ClaimTypes.Email,loginResponse.CurrentUserEmailAddress)
        //    , new(JwtRegisteredClaimNames.UniqueName, loginResponse.CompanyId.ToString())
        //    , new(StellaJwtConstants.EmployeeId, loginResponse.EmployeeId)
        //};

        private string GetRole()
        {
            var accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }

#nullable enable
        private string? GetId(string? companyId = null)
        {
            var role = GetRole();

            if (role == AuthRoles.Owner)
                return Id;

            return companyId;
        }

        private void RequireCompanyId(string companyId)
        {
            var role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(companyId))
                throw new HttpResponseException(new Response(error: "Company id is not specified!")); ;

        }

#nullable disable

    }
}
