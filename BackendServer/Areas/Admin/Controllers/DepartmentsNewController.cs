using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using System.Collections.Generic;
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
    public class DepartmentsNewController : BaseController
    {
        private readonly IDepartmentsNewActions actions;
        public DepartmentsNewController(IDepartmentsNewActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<CompanyDepartments>), 200)]
        public async Task<Response> AddDepartments([FromBody] CompanyDepartments model, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            string role = GetRole();
            Response response = await actions.AddDepartments(createdBy, updatedBy, model, role, companyid);

            if (response is not Response<CompanyDepartments> correspondingResponse) return response;

            return new Response<CompanyDepartments>(correspondingResponse.Data!);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Response<CompanyDepartments>), 200)]
        public async Task<Response> UpdateDepartment([FromBody] CompanyDepartments model, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            Response response = await actions.UpdateDepartment(updatedBy, model, role, companyid);

            if (response is not Response<CompanyDepartments> correspondingResponse) return response;

            return new Response<CompanyDepartments>(correspondingResponse.Data!);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<CompanyDepartments>), 200)]
        public async Task<Response> GetDepartments([FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string role = GetRole();

            return await actions.GetDepartments(role, companyid);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<List<CompanyDepartments>>), 200)]
        public async Task<Response> GetDepartment([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string role = GetRole();

            return await actions.GetDepartment(id, role, companyid);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<List<CompanyDepartments>>), 200)]
        public async Task<Response> SoftDeleteDepartment([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            return await actions.SoftDeleteDepartment(updatedBy, id, role, companyid);
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

        private void RequireCompanyId(string memberId)
        {
            var role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(memberId))
                throw new HttpResponseException(new Response(error: "Company id is not specified!")); ;

        }

#nullable disable

    }
}
