using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PrePurchase.Models;
using System;
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
    public class LoansController : BaseController
    {

        [HttpPost]
        [ProducesResponseType(typeof(Response<Loan>), 200)]
        public async Task<Response> AddnewLoan([FromQuery][Required] string employeeId, RequestLoanModel model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            string createdBy = updatedBy;
            return await _loansActions.AddnewLoan(createdBy, updatedBy, employeeId, model, role, companyId);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<List<Loan>>), 200)]
        public async Task<Response> GetLoans([FromQuery] QueryLoanModel model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            var role = GetRole();
            model = model with { CompanyId = companyId };

            return await _loansActions.GetLoans(model, role);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(Response<Loan>), 200)]
        public async Task<Response> UpdateLoan([FromRoute][Required] string id, [Required] CompanyUpdateLoan model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await _loansActions.UpdateLoan(updatedBy, id, model, role, companyId);
        }


        public LoansController(ILoansActions loansActions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _loansActions = loansActions;

        }

        private string GetRole()
        {
            string[] accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }



#nullable enable
        private string? GetLoggedUserId(string? userId = null)
        {
            var role = GetRole();

            if (role == AuthRoles.Owner)
                return Id;

            return userId;
        }

        private void RequireUserId(string userId)
        {
            var role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(userId))
                throw new HttpResponseException(new Response(error: "Company id is not specified!")); ;

        }

#nullable disable

        private readonly ILoansActions _loansActions;
    }
}
