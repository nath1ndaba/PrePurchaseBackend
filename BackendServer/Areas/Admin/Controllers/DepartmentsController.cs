using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrePurchase.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

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
    public class DepartmentsController : BaseController
    {

        [HttpGet]
        [ProducesResponseType(typeof(Response<List<string>>), 200)]
        public async Task<Response> GetDepartments([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            string role = GetRole();
            Response response = await _departmentsActions.GetDepartments(role, companyId);

            return response;
        }

        [HttpPut("{department}")]
        [ProducesResponseType(typeof(Response<string>), 201)]
        public async Task<Response> PutDepartment([FromRoute][Required] string department, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            string role = GetRole();
            Response response = await _departmentsActions.AddDepartment(department, role, companyId);
            return response;
        }

        [HttpDelete("{department}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> DeleteDepartment([FromRoute][Required] string department, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            string role = GetRole();
            Response response = await _departmentsActions.RemoveDepartment(department, role, companyId);
            return response;
        }


        public DepartmentsController(IDepartmentsActions companyActions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _departmentsActions = companyActions;

        }

        private string GetRole()
        {
            string[] accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }
        private readonly IDepartmentsActions _departmentsActions;
    }
}
