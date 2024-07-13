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
using PrePurchase.Models.Requests;
using PrePurchase.Models.StatementsModels;
using PrePurchase.Models.Extractors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
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
    [Authorize(Policy = AuthPolicies.Company)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class UniversalController : BaseController
    {
        private readonly IUniversalActions actions;
        public string CurrentUserEmail { get; set; }

        public UniversalController(IUniversalActions actions
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
        public async Task<Response> Login([FromBody] UniversalLoginModel model)
        {
            Response response = await actions.Login(model.Sanitize());

            if (response is not Response<UniversalLoginResponse> companyResponse)
                return response;

            UniversalLoginResponse universalLoginResponse = companyResponse.Data;
            return new Response<JwtTokenModel>(await GetAuthTokens(universalLoginResponse));
        }

        [HttpGet("GetPositionsInfo")]
        [ProducesResponseType(typeof(Response<IEnumerable<UniversalPositionsInfo>>), 200)]
        public async Task<Response> GetPositionsInfo()
        {
            //RequireUserId(companyId);
            //companyId = GetCompanyId(companyId);

            string role = GetRole();
            return await actions.GetPositionsInfo(role, Id);
        }

        [HttpPost("UniversalClockings")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> ClockInOut([FromBody] UniversalClockingModel model, [FromQuery] string? companyId = null)
        {
            string role = GetRole();
            return await actions.UniversalClockings(model, role, Id);
        }



        [HttpPost("ChangeEmployeePassword")]
        [ProducesResponseType(typeof(Response<ChangePasswordModel>), 200)]
        public async Task<Response> ChangeEmployeePassword(string employeeDetailsId, string password)
        {
            return await actions.ChangeEmployeePassword(employeeDetailsId, password);
        }

        [HttpPatch("ChangeEmployeePin")]
        [ProducesResponseType(typeof(Response<ChangePin>), 200)]
        public async Task<Response> ChangeEmployeePin(ChangePin changePin)
        {
            return await actions.ChangeEmployeePin(changePin);
        }



        private Task<JwtTokenModel> GetAuthTokens(UniversalLoginResponse response)
        {
            Claim[] claims = {
                new(ClaimTypes.Role, AuthRoles.Owner)
                , new (ClaimTypes.Email,response.Email)
                , new(JwtRegisteredClaimNames.UniqueName, response.UserCompany.Id.ToString())};

            return GetAuthTokens(claims);
        }

        private string GetRole()
        {
            string[] acceptedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this function is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => acceptedRoles.Contains(x.Value)).Value;

        }
    }
}
