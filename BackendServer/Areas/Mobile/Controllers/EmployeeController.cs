using BackendServices;
using BackendServices.Actions;
using BackendServices.JWT;
using BackendServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using PrePurchase.Models.LeaveDays;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("api/v1")]
    [Route("[area]/[controller]")]
    [Authorize(Roles = AuthRoles.User)]
    [ApiController]
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeActions actions;

        protected string UserId
            => User.FindFirstValue(PrePurchaseJwtConstants.UserId);

        public EmployeeController(
            IAuthContainerModel containerModel
            , IAuthService authService
            , IRepository<RefreshToken> refreshTokens
            , IEmployeeActions actions)
            : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<EmployeeDetails>), 200)]
        public async Task<Response> Get()
        {
            return await actions.FindById(Id);
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<JwtTokenModel>), 200)]
        public async Task<Response> Login([FromBody] EmployeeLoginModel model)
        {
            var response = await actions.Login(model.Sanitize());

            if (response is not Response<EmployeeDetails> employeeResponse)
                return response;

            var employee = employeeResponse.Data!;
            // generate tokens

            return new Response<JwtTokenModel>(await GetAuthTokens(employee));
        }

        [HttpPost("changePassword")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> ChangePassword([FromBody] ChangePasswordModel model)
        {
            return await actions.ChangePassword(Id, model);
        }

        [HttpPost("clockin")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> ClockInOut([FromBody] ClockInAndOutData clockInData)
        {
            return await actions.ClockInOut(clockInData, UserId, Id);
        }

        [HttpGet("GetClockedInStatus")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> GetClockedInStatus([FromQuery][Required] string companyId)
        {
            return await actions.GetClockedInStatus(companyId, Id);
        }

        [HttpGet("GetDetailedAds")]
        [ProducesResponseType(typeof(Response<IEnumerable<DetailedAd>>), 200)]
        public async Task<Response> GetDetailedAds()
        {
            return await actions.GetDetailedAds();
        }

        [HttpGet("histories")]
        [ProducesResponseType(typeof(Response<IEnumerable<History>>), 200)]
        public async Task<Response> GetHistories([FromQuery] string companyId, [FromQuery] int page = 0, [FromQuery] int limit = 0)
        {
            var skip = page * limit;
            return await actions.HistoriesByEmployeeDetailsId(Id, companyId, skip, limit);
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(Response<History>), 200)]
        public async Task<Response> GetHistory([FromQuery][Required] string historyId)
        {
            var response = (Response<History>)await actions.HistoryById(Id, historyId);

            if (response.Data is null || response.Data!.EmployeeDetailsId.ToString() == Id)
                return response;

            return new Response(HttpStatusCode.Forbidden, "You are not allowed to access this history!");
        }

        [HttpGet("loans")]
        [ProducesResponseType(typeof(Response<IEnumerable<Loan>>), 200)]
        public async Task<Response> GetLoans([FromQuery][Required] string companyId, [FromQuery] int page = 0, [FromQuery] int limit = 0)
        {
            var skip = page * limit;
            return await actions.LoansByEmployeeDetailsId(companyId, Id, skip, limit);
        }

        [HttpPut("loan")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> AddLoan([FromQuery][Required] string companyId, RequestLoanModel model)
        {
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            return await actions.RequestLoan(createdBy, updatedBy, model, UserId, companyId);
        }

        [HttpDelete("loan/{loanId}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> RemoveLoan([FromQuery][Required] string companyId, [FromRoute][Required] string loanId)
        {
            return await actions.RemoveLoan(companyId, loanId, UserId);
        }

        [HttpGet("loan")]
        [ProducesResponseType(typeof(Response<Loan>), 200)]
        public async Task<Response> GetLoan([FromQuery][Required] string loanId)
        {
            var response = (Response<Loan>)await actions.LoanById(Id, loanId);

            if (response.Data is null || response.Data!.EmployeeSummary.Id.ToString() == Id)
                return response;

            return new Response(HttpStatusCode.Forbidden, "You are not allowed to access this loan!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        /// 
        [HttpGet("leaves")]
        [ProducesResponseType(typeof(Response<IEnumerable<Leave>>), 200)]
        public async Task<Response> GetLeaves([FromQuery][Required] string companyId, [FromQuery] int page = 0, [FromQuery] int limit = 0)
        {
            var skip = page * limit;
            return await actions.LeavesByEmployeeDetailsId(companyId, Id, skip, limit);
        }

        [HttpGet("GetleaveStore/{employeeId}")]
        [ProducesResponseType(typeof(Response<LeaveStore>), 200)]
        public async Task<Response> LeaveStoreByEmployeeId([FromQuery][Required] string companyId, string employeeId)
        {
            return await actions.LeaveStoreByEmployeeId(companyId, employeeId);
        }

        [HttpPut("leave")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> AddLeave([FromQuery][Required] string companyId, RequestLeaveModel model)
        {
            LeaveStatus status = LeaveStatus.New;
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;
            return await actions.RequestLeave(createdBy, updatedBy, status, model, UserId, companyId);
        }

        [HttpDelete("leave/{leaveId}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> RemoveLeave([FromQuery][Required] string companyId, [FromRoute][Required] string leaveId)
        {
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);
            return await actions.RemoveLeave(updatedBy, companyId, leaveId, UserId);
        }

        [HttpGet("leave")]
        [ProducesResponseType(typeof(Response<Leave>), 200)]
        public async Task<Response> GetLeave([FromQuery][Required] string leaveId)
        {
            return await actions.LeaveById(Id, leaveId);
        }

        [HttpGet("rosta/{companyId}")]
        [ProducesResponseType(typeof(Response<IDictionary<string, List<EmployeeTask>>>), 200)]
        public async Task<Response> GetRosta([FromRoute][Required] string companyId)
        {
            return await actions.Rosta(UserId, companyId);
        }

        [HttpGet("companyProfiles")]
        [ProducesResponseType(typeof(Response<List<CompanyEmployeeProfile>>), 200)]
        public async Task<Response> GetCompnayProfiles()
        {
            return await actions.CompanyProfiles(UserId);
        }

        [HttpGet("timeSummaries")]
        [ProducesResponseType(typeof(Response<List<TimeSummary>>), 200)]
        public async Task<Response> GetTimeSummaries([FromQuery] int page = 0, [FromQuery] int limit = 0)
        {
            var skip = page * limit;
            return await actions.TimeSummariesByEmployeeDetailsId(Id, skip, limit);
        }

        [HttpGet("timeSummary/{timeSummaryId}")]
        [ProducesResponseType(typeof(Response<TimeSummary>), 200)]
        public async Task<Response> GetTimeSummary([FromRoute][Required] string timeSummaryId)
        {
            return await actions.TimeSummaryById(Id, timeSummaryId);
        }
        [HttpGet("getmobileWallet")]
        [ProducesResponseType(typeof(Response<MobileWallet>), 200)]
        public async Task<Response> MobileWallet([Required] string companyId)
        {
            return await actions.MobileWalletByEmployeeDetailsId(Id, companyId);
        }

        private Task<JwtTokenModel> GetAuthTokens(EmployeeDetails employee)
        {
            Claim[] claims = {
                new(ClaimTypes.Role, AuthRoles.User)
                , new(JwtRegisteredClaimNames.UniqueName, employee.Id.ToString())
            , new(PrePurchaseJwtConstants.UserId, employee.EmployeeId)};

            return GetAuthTokens(claims);
        }
    }
}
