using BackendServices;
using BackendServices.Actions;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOG.Models;
using PrePurchase.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("simunye")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Shop)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class SimunyeController : BaseController
    {
        private readonly ISimunyeActions actions;
        public static string CurrentUserEmail { get; set; }

        public SimunyeController(ISimunyeActions actions
            , IAuthContainerModel containerModel
            , IAuthService authService
            , IRepository<RefreshToken> refreshTokens)
            : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }


        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> Login([FromBody] MemberLogin model)
        {
            var response = await actions.Login(model);

            if (response is Response<SOGMembers> companyResponse)
                return new Response<JwtTokenModel>(await GetAuthTokens(companyResponse.Data!));

            else
                return response;

            // generate tokens

        }

        [HttpPost("Members")]
        [ProducesResponseType(typeof(Response<SOGMembers>), 200)]
        public async Task<Response> RegisterMember([FromBody] SOGMembers model)
        {
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await actions.AddMember(createdBy, updatedBy, model);

            if (response is not Response<SOGMembers> memberResponse)
                return response;

            return new Response<SOGMembers>(memberResponse.Data!);
        }

        [HttpPost("Departments")]
        [ProducesResponseType(typeof(Response<SOGDepartments>), 200)]
        public async Task<Response> AddDepartment([FromBody] SOGDepartments model)
        {
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await actions.AddDepartments(createdBy, updatedBy, model);

            if (response is not Response<SOGDepartments> departmentResponse)
                return response;

            return new Response<SOGDepartments>(departmentResponse.Data!);
        }

        [HttpPost("Positions")]
        [ProducesResponseType(typeof(Response<SOGPositions>), 200)]
        public async Task<Response> AddPositions([FromBody] SOGPositions model)
        {
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await actions.AddPositions(createdBy, updatedBy, model);

            if (response is not Response<SOGDepartments> positionResponse)
                return response;

            return new Response<SOGDepartments>(positionResponse.Data!);
        }

        [HttpPut("Members")]
        [ProducesResponseType(typeof(Response<SOGMembers>), 200)]
        public async Task<Response> UpdateMember([FromBody] SOGMembers model)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            Response response = await actions.UpdateMember(updatedBy, model);

            if (response is not Response<SOGMembers> memberResponse)
                return response;

            return new Response<SOGMembers>(memberResponse.Data!);
        }


        [HttpPut("Departments")]
        [ProducesResponseType(typeof(Response<SOGDepartments>), 200)]
        public async Task<Response> UpdateDepartment([FromBody] SOGDepartments model)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            Response response = await actions.UpdateDepartment(updatedBy, model);

            if (response is not Response<SOGDepartments> positionResponse)
                return response;

            return new Response<SOGDepartments>(positionResponse.Data!);
        }

        [HttpPut("Positions")]
        [ProducesResponseType(typeof(Response<SOGDepartments>), 200)]
        public async Task<Response> UpdatePosition([FromBody] SOGPositions model)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            Response response = await actions.UpdatePosition(updatedBy, model);

            if (response is not Response<SOGPositions> positionResponse)
                return response;

            return new Response<SOGPositions>(positionResponse.Data!);
        }

        [HttpGet("Members")]
        [ProducesResponseType(typeof(Response<SOGMembers>), 200)]
        public async Task<Response> Getmembers([FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);

            var role = GetRole();
            memberId = GetId(memberId);

            return await actions.GetMembers(role, memberId);
        }

        [HttpGet("Departments")]
        [ProducesResponseType(typeof(Response<SOGDepartments>), 200)]
        public async Task<Response> GetDepartments([FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);

            string role = GetRole();
            memberId = GetId(memberId);

            return await actions.GetDepartments(role, memberId);
        }


        [HttpGet("Positions")]
        [ProducesResponseType(typeof(Response<SOGPositions>), 200)]
        public async Task<Response> GetPositions([FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);

            string role = GetRole();
            memberId = GetId(memberId);

            return await actions.GetPositions(role, memberId);
        }

        [HttpGet("Izifo")]
        [ProducesResponseType(typeof(Response<SOGIzifo>), 200)]
        public async Task<Response> GetIzifo([FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);

            var role = GetRole();
            memberId = GetId(memberId);

            return await actions.GetIzifo(role, memberId);
        }

        [HttpGet("Contributions/{isifoId}")]
        [ProducesResponseType(typeof(Response<List<SOGContributedMember>>), 200)]
        public async Task<Response> GetContributions([FromRoute] string isifoId, [FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);

            string role = GetRole();
            memberId = GetId(memberId);

            return await actions.GetContributions(role, isifoId, memberId);
        }

        [HttpGet("getLoggedMemberInfo")]
        [ProducesResponseType(typeof(Response<SOGMembers>), 200)]
        public async Task<Response> getLoggedMemberInfo([FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);

            var role = GetRole();
            memberId = GetId(memberId);

            return await actions.Get(role, memberId);
        }

        [HttpPost("Izifo")]
        [ProducesResponseType(typeof(Response<SOGIzifo>), 201)]
        public async Task<IActionResult> AddIsifo([FromBody][Required] SOGIzifo model, [FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);
            memberId = GetId(memberId);

            string role = GetRole();
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await actions.AddIsifo(createdBy, updatedBy, model, role, GetId(memberId));

            return StatusCode((int)response.StatusCode, response);

        }

        [HttpPost("Contributions")]
        [ProducesResponseType(typeof(Response<SOGContribution>), 201)]
        public async Task<IActionResult> ContributedMembers([FromBody][Required] SOGContribution contribution, [FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);
            memberId = GetId(memberId);

            string role = GetRole();
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await actions.AddContribution(createdBy, updatedBy, contribution, role, GetId(memberId));

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("Contributions")]
        [ProducesResponseType(typeof(Response<SOGContribution>), 201)]
        public async Task<IActionResult> UpdateContribution([FromBody][Required] SOGContribution contribution, [FromQuery] string memberId = null)
        {
            RequireCompanyId(memberId);
            memberId = GetId(memberId);

            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            Response response = await actions.UpdateContribution(updatedBy, contribution);

            return StatusCode((int)response.StatusCode, response);
        }


        [HttpDelete("Departments/{id}")]
        [ProducesResponseType(typeof(Response<SOGDepartments>), 200)]
        public async Task<Response> SoftDeleteDepartment([FromRoute] string id)
        {
            string deletedBy = User.FindFirstValue(ClaimTypes.Name);
            return await actions.SoftDeleteDepartment(deletedBy, id);
        }

        [HttpDelete("Positions/{id}")]
        [ProducesResponseType(typeof(Response<SOGPositions>), 200)]
        public async Task<Response> SoftDeletePosition([FromRoute] string id)
        {
            string deletedBy = User.FindFirstValue(ClaimTypes.Name);
            return await actions.SoftDeletePosition(deletedBy, id);
        }

        [HttpDelete("members/{id}")]
        [ProducesResponseType(typeof(Response<SOGDepartments>), 200)]
        public async Task<Response> SoftDeleteMember([FromRoute] string id)
        {
            string deletedBy = User.FindFirstValue(ClaimTypes.Name);
            return await actions.SoftDeleteMember(deletedBy, id);
        }

        [HttpDelete("izifo/{id}")]
        [ProducesResponseType(typeof(Response<SOGDepartments>), 200)]
        public async Task<Response> SoftDeleteIsifo([FromRoute] string id)
        {
            string deletedBy = User.FindFirstValue(ClaimTypes.Name);
            return await actions.SoftDeleteIsifo(deletedBy, id);
        }

        [HttpDelete("contributions/{id}")]
        [ProducesResponseType(typeof(Response<SOGContribution>), 200)]
        public async Task<Response> SoftDeleteContribution([FromRoute] string id)
        {
            string deletedBy = User.FindFirstValue(ClaimTypes.Name);
            return await actions.SoftDeleteContribution(deletedBy, id);
        }

        private Task<JwtTokenModel> GetAuthTokens(SOGMembers member)
        {

            Claim[] claims = {
                new(ClaimTypes.Role, AuthRoles.Owner)
                , new (ClaimTypes.Email,member.Login.ContactNumber)
                , new(JwtRegisteredClaimNames.UniqueName, member.Id.ToString())};

            return GetAuthTokens(claims);
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
