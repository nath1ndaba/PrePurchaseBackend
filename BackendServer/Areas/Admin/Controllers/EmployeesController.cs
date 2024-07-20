using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.Requests;
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
    [Authorize(Policy = AuthPolicies.Shop)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class EmployeesController : BaseController
    {

        [HttpGet("employees")]
        [ProducesResponseType(typeof(Response<CompanyEmployee>), 200)]
        public async Task<Response> GetEmployees([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();

            return await _actions.GetEmployees(role, companyId);
        }

        [HttpGet("archivedEmployees")]
        [ProducesResponseType(typeof(Response<CompanyEmployee>), 200)]
        public async Task<Response> GetArchivedEmployees([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();

            return await _actions.GetArchivedEmployees(role, companyId);
        }

        [HttpGet("employeesByDepartment")]
        [ProducesResponseType(typeof(Response<List<CompanyEmployee>>), 200)]
        public async Task<Response> GetEmployeesByDepartment([FromQuery][Required] string department, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();

            return await _actions.GetEmployeesByDepartment(department, role, companyId);
        }

        [HttpPatch("employee/{employeeId}")]
        [ProducesResponseType(typeof(Response<EmployeeDetails>), 200)]
        [ProducesResponseType(typeof(Response), 400)]
        [ProducesResponseType(typeof(Response), 404)]
        public async Task<Response> UpdateEmployeeInfo([FromRoute][Required] string employeeId, [Required] UpdateEmployeeRequest model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _actions.UpdateEmployee(employeeId, model, updatedBy, role, companyId);
        }

        [HttpPost("AddExistingEmployeeToNewCompany")]
        [ProducesResponseType(typeof(Response<AddExistingEmployeeToNewCompany>), 201)]
        public async Task<IActionResult> AddExistingEmployeeToNewCompany([FromBody][Required] AddExistingEmployeeToNewCompany model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await _actions.AddExistingEmployeeToNewCompany(model, companyId);

            return StatusCode((int)response.StatusCode, response);

        }

        [HttpPost("employee")]
        [ProducesResponseType(typeof(Response<AddNewEmployeeResponseModel>), 201)]
        public async Task<IActionResult> AddEmployee([FromBody][Required] AddEmployeeModel model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await _actions.AddEmployee(model, createdBy, updatedBy, role, companyId);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("unarchiveEmployee")]
        [ProducesResponseType(typeof(Response<AddNewEmployeeResponseModel>), 201)]
        public Task<Response> UnArchiveEmployee([FromQuery][Required] string employeeId, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            return _actions.UnArchiveEmployee(employeeId, updatedBy, companyId);
        }

        [HttpPost("ImportEmployees")]
        [ProducesResponseType(typeof(Response<List<AddNewEmployeeResponseModel>>), 201)]
        public async Task<IActionResult> ImportEmployees([FromBody][Required] List<AddEmployeeModel> employees, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            string createdBy = User.FindFirstValue(ClaimTypes.Name);

            string updatedBy = createdBy;
            Response response = await _actions.ImportEmployees(employees, createdBy, updatedBy, role, companyId);

            return StatusCode((int)response.StatusCode, response);
        }


        [HttpPatch("employee/ResetEmployeeDevice/{employeeId}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> ResetEmployeeDevice([FromRoute][Required] string employeeId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _actions.ResetEmployeeDevice(employeeId, updatedBy);

        }

        [HttpPatch("employee/ResetEmployeePassword/{employeeId}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> ResetEmployeePassword([FromRoute][Required] string employeeId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);

            return await _actions.ResetEmployeePassword(employeeId, updatedBy);
        }

        [HttpDelete("employee/{id}")]
        [ProducesResponseType(typeof(Response<EmployeeDetails>), 200)]
        public async Task<Response> SoftDeleteEmployee([FromRoute] string id, string companyId)
        {
            string deletedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _actions.SoftDeleteEmployee(deletedBy, id, companyId);
        }

        public EmployeesController(IEmployeesActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _actions = actions;
        }

        private string GetRole()
        {
            string[] accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }

#nullable enable
        private string? GetId(string? companyId = null)
        {
            string role = GetRole();

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

        private readonly IEmployeesActions _actions;
    }
}
