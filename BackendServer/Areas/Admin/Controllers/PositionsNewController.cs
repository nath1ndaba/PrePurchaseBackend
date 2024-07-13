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
    [Authorize(Policy = AuthPolicies.Company)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class PositionsNewController : BaseController
    {
        private readonly IPositionsNewActions actions;
        public PositionsNewController(IPositionsNewActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<CompanyPositions>), 200)]
        public async Task<Response> AddPosition([FromBody] CompanyPositions model, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            string role = GetRole();
            Response response = await actions.AddPosition(createdBy, updatedBy, model, role, companyid);

            if (response is not Response<CompanyPositions> correspondingResponse) return response;

            return new Response<CompanyPositions>(correspondingResponse.Data!);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Response<CompanyPositions>), 200)]
        public async Task<Response> UpdatePosition([FromBody] CompanyPositions model, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            Response response = await actions.UpdatePosition(updatedBy, model, role, companyid);

            if (response is not Response<CompanyPositions> correspondingResponse) return response;

            return new Response<CompanyPositions>(correspondingResponse.Data!);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<CompanyPositions>), 200)]
        public async Task<Response> GetPositions([FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string role = GetRole();

            return await actions.GetPositions(role, companyid);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<List<CompanyPositions>>), 200)]
        public async Task<Response> GetPosition([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string role = GetRole();

            return await actions.GetPosition(id, role, companyid);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<List<CompanyPositions>>), 200)]
        public async Task<Response> SoftDeletePosition([FromRoute] string id, [FromQuery] string companyid = null)
        {
            RequireCompanyId(companyid);
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string role = GetRole();

            return await actions.SoftDeletePosition(updatedBy, id, role, companyid);
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
