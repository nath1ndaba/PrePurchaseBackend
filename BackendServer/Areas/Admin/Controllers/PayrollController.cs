using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.HistoryModels;
using PrePurchase.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
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
    public class PayrollController : BaseController
    {
        private readonly IPayrollActions _actions;
        public string CurrentUserEmail { get; set; }

        public PayrollController(IPayrollActions actions
            , IAuthContainerModel containerModel
            , IAuthService authService
            , IRepository<RefreshToken> refreshTokens)
            : base(containerModel, authService, refreshTokens)
        {
            _actions = actions;
        }


        [HttpGet("GetProcessedTimesSummaries")]
        [ProducesResponseType(typeof(Response<IEnumerable<ProcessedTimesSummary>>), 200)]
        public async Task<Response> GetprocessedTimesSummaries([FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            return await _actions.GetprocessedTimesSummaries(role, companyId);
        }

        [HttpGet("GetProcessedTimesSummariesBatch/{batchCode}")]
        [ProducesResponseType(typeof(Response<ProcessedTimesSummary>), 200)]
        public async Task<Response> GetProcessedTimesSummariesBatch([FromRoute][Required] string batchCode, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            return await _actions.GetProcessedTimesSummariesBatch(role, batchCode, companyId);
        }

        [HttpGet("UndoProcessedTimesSummariesBatch/{batchCode}")]
        [ProducesResponseType(typeof(Response<ProcessedTimesSummary>), 200)]
        public async Task<Response> UndoProcessedTimesSummariesBatch([FromRoute][Required] string batchCode, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _actions.UndoProcessedTimesSummariesBatch(updatedBy, role, batchCode, companyId);
        }

        [HttpPatch("AdminUpdateClockings")]
        [ProducesResponseType(typeof(Response<AdminManualClockings>), 200)]
        public async Task<Response> ClockEmployeeViaAdmin([FromBody][Required] AdminManualClockings model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _actions.ClockEmployeeViaAdmin(updatedBy, model, role, companyId);
        }

        [HttpGet("timeSummaries")]
        [ProducesResponseType(typeof(Response<List<TimeSummary>>), 200)]
        public async Task<Response> GetTimeSummaries([FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();

            return await _actions.TimeSummariesByCompanyId(role, companyId);
        }

        [HttpGet("timeSummariesForRange")]
        [ProducesResponseType(typeof(Response<List<TimeSummaryWithEmployeeDetails>>), 200)]
        public async Task<Response> GetTimeSummariesForRange([FromQuery][Required] TimeSummariesForRangeModel model, [Required] string? companyId = null)
        {
            string role = GetRole();
            model = model with { CompanyId = companyId };

            return await _actions.TimeSummariesForRangeByCompanyId(role, model);
        }


        [HttpPut("AmmendClocks")]
        [ProducesResponseType(typeof(Response), 201)]
        public async Task<Response> AmendClockings(List<AmendClocks> amendClocks, string employeeDetailsId, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            if (amendClocks is null) throw new ArgumentNullException($"{nameof(amendClocks)} may not be null");
            if (employeeDetailsId is null) throw new ArgumentNullException($"{nameof(employeeDetailsId)} may not be null");

            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _actions.AmendClockings(updatedBy, amendClocks, employeeDetailsId, role, companyId);
        }


        [HttpPut("OverrideClocks")]
        [ProducesResponseType(typeof(Response), 201)]
        public async Task<Response> OverrideClocks(string employeeDetailsId, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _actions.OverrideClockings(updatedBy, employeeDetailsId, role, companyId);
        }


        [HttpPut("OverrideAllClockings")]
        [ProducesResponseType(typeof(Response), 201)]
        public async Task<Response> OverrideAllClockings([FromQuery][Required] string companyid = null)
        {
            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            
            return await _actions.OverrideAllClockings(updatedBy, role, companyid);
        }


        [HttpPost("StoreProcessedPayrollBatch")]
        [ProducesResponseType(typeof(Response<ProcessedPayrollBatch>), 201)]
        public async Task<IActionResult> StoreProcessedPayroll([FromBody][Required] BatchRequest model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (email is null)
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized));

            if (model is null || model.PayrollEmployeesIds.Count() < 1)
                throw new HttpResponseException(new Response(HttpStatusCode.BadRequest));
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await _actions.StoreProcessedPayrollBatch(createdBy, updatedBy, role, model, email, companyId);

            return StatusCode((int)response.StatusCode, response);
        }


        [HttpPut("UpdateProcessedPayrollBatch")]
        [ProducesResponseType(typeof(Response<ProcessedPayrollBatch>), 201)]
        public async Task<IActionResult> UpdateProcessedPayrollBatch([FromQuery][Required] string BatchCode, [FromBody] List<AdjustedValuesOnPay> model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (email is null) throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized));

            if (model is null || model.Count < 1) throw new HttpResponseException(new Response(HttpStatusCode.Forbidden));
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            Response response = await _actions.UpdateProcessedPayrollBatch(updatedBy, role, BatchCode, model, email, companyId);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("GetProcessedPayrollBatch")]
        [ProducesResponseType(typeof(Response<IEnumerable<History>>), 200)]
        public async Task<Response> GetProcessedPayrollBatch([FromQuery] string? companyId = null)
        {
            string role = GetRole();

            return await _actions.GetProcessedPayrollBatch(role, companyId);
        }

        [HttpPost("ApplyLeaveViaAdmin")]
        [ProducesResponseType(typeof(Response<RequestLeaveModel>), 201)]
        public async Task<IActionResult> ApplyLeaveViaAdmin([FromQuery][Required] string employeeId, [FromBody][Required] RequestLeaveModel model, [FromQuery][Required] string? companyId = null)
        {
            string role = GetRole();
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            Response response = await _actions.ApplyLeaveViaAdmin(createdBy, updatedBy, model, employeeId, companyId, role);
            return StatusCode((int)response.StatusCode, response);
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
            string role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(companyId))
                throw new HttpResponseException(new Response(error: "Company id is not specified!")); ;

        }

#nullable disable

    }
}
