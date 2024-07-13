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
    public class CompanyController : BaseController
    {

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> Register([FromBody] AdminRegisterModel model)
        {
            Response response = await _companyActions.Register(model);

            return response;
        }


        [HttpGet]
        [ProducesResponseType(typeof(Response<Company>), 200)]
        public async Task<Response> GetCompany([FromQuery][Required] string companyId)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            string role = GetRole();

            var response = await _companyActions.GetCompany(role, companyId);

            return response;
        }

        [HttpPatch]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> Update([FromBody] CompanyUpdateModel model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            return await _companyActions.Update(model, role, companyId);
        }


        [HttpPatch("AddCompanySites")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> AddCompanySites([FromBody] Location model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            return await _companyActions.AddCompanySites(model, role, companyId);
        }



        [HttpGet("deductions")]
        [ProducesResponseType(typeof(Response<List<Deduction>>), 200)]
        public async Task<Response> GetDeductions([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            Response response = await GetCompany(companyId);

            if (response is not Response<Company> companyResponse)
                return response;

            return new Response<List<Deduction>>(companyResponse.Data!.Deductions);
        }

        [HttpPost("deduction")]
        [ProducesResponseType(typeof(Response<Deduction>), 201)]
        public async Task<Response> AddDeduction([FromBody] DeductionModel model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            return await _companyActions.AddDeductions(model, role, companyId);

        }

        [HttpPut("deduction")]
        [ProducesResponseType(typeof(Response<Deduction>), 201)]
        public async Task<Response> UpdateDeduction([FromBody] DeductionModel model, [FromQuery][Required] string? companyId = null, [FromQuery] string Id = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            return await _companyActions.UpdateDeduction(Id, model, role, companyId);


        }

        [HttpDelete("deduction/{id}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> DeleteDeduction([FromRoute][Required] string id, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            string role = GetRole();
            return await _companyActions.RemoveDeduction(id, role, companyId);

        }


        [HttpGet("positions")]
        [ProducesResponseType(typeof(Response<List<string>>), 200)]
        public async Task<Response> GetPostions([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            Response response = await GetCompany(companyId);

            if (response is not Response<Company> companyResponse)
                return response;

            return new Response<List<string>>(companyResponse.Data!.Positions);

        }

        [HttpPut("position")]
        [ProducesResponseType(typeof(Response<string>), 201)]
        public async Task<Response> PutPosition([FromBody][Required] string position, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            return await _companyActions.AddPosition(position, role, companyId);

        }

        [HttpDelete("position/{position}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> DeletePosition([FromQuery][Required] string position, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            return await _companyActions.RemovePosition(position, role, companyId);

        }

        [HttpGet("rates")]
        [ProducesResponseType(typeof(Response<List<Rate>>), 200)]
        public async Task<Response> GetRates([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            Response response = await GetCompany(companyId);

            if (response is not Response<Company> companyResponse)
                return response;
            return new Response<List<Rate>>(companyResponse.Data!.Rates);
        }

        [HttpPost("rate")]
        [ProducesResponseType(typeof(Response<Rate>), 201)]
        public async Task<Response> PutRate([FromBody][Required] RateModel model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            string role = GetRole();
            Response response = await _companyActions.AddRate(model.Sanitize(), role, companyId);

            return response;
        }



        [HttpPut("rate")]
        [ProducesResponseType(typeof(Response<Rate>), 202)]
        public async Task<Response> UpdateRate([FromBody] RateModel model, [FromQuery][Required] string? companyId = null, [FromQuery][Required] string Id = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            var role = GetRole();
            Response response = await _companyActions.UpdateRate(Id, model, role, companyId);
            return response;
        }

        [HttpPut("updateShift")]
        [ProducesResponseType(typeof(Response<Shift>), 202)]
        public async Task<Response> UpdateShift([FromBody] ShiftModel model, [FromQuery][Required] string? companyId = null, [FromQuery] string Id = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            var role = GetRole();
            Response response = await _companyActions.UpdateShift(Id, model, role, companyId);
            return response;
        }


        [HttpDelete("rate/{id}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> DeleteRate([FromRoute][Required] string id, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            if (id is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "id must have a value😒"));

            string role = GetRole();
            Response response = await _companyActions.RemoveRate(id, role, companyId);
            return response;

        }

        [HttpGet("shifts")]
        [ProducesResponseType(typeof(Response<List<Shift>>), 200)]
        public async Task<Response> GetShifts([FromQuery] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            string role = GetRole();
            Response response = await _companyActions.GetCompany(role, companyId);

            if (response is not Response<Company> companyResponse)
                return response;

            return new Response<List<Shift>>(companyResponse.Data!.Shifts);

        }

        [HttpPut("shift")]
        [ProducesResponseType(typeof(Response<Shift>), 201)]
        public async Task<Response> PutShift([FromBody][Required] ShiftModel model, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            if (model is null) throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "shifts can not be null😒"));

            string role = GetRole();
            Response response = await _companyActions.AddShift(model.Sanitize(), role, companyId);
            return response;
        }

        [HttpDelete("shift/{id}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> DeleteShift([FromRoute][Required] string id, [FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            string role = GetRole();
            Response response = await _companyActions.RemoveShift(id, role, companyId);
            return response;
        }

        [HttpGet("SuppliersPaymentMethod")]
        [ProducesResponseType(typeof(Response<List<string>>), 200)]
        public async Task<Response> GetPaymentMethod([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));

            Response response = await GetCompany(companyId);

            if (response is not Response<Company> companyResponse)
                return response;

            return new Response<List<string>>(companyResponse.Data!.SuppliersPaymentMethods);
        }

        public CompanyController(ICompanyActions companyActions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _companyActions = companyActions;

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

        private readonly ICompanyActions _companyActions;
    }
}
