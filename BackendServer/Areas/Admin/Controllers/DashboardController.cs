using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Actions.Admin.RealTimeServices;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Infrastructure.Hubs;
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
    [Authorize(Policy = AuthPolicies.Company)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class DashboardController : BaseController
    {

        [HttpPost]
        public void OnClockingAction([FromQuery][Required] string? companyId = null)
        {
            if (companyId is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "CompanyId must have a value😒"));
            _realTimeDashboard.OnClockingAction(companyId);
        }

        public DashboardController(IRealTimeDashBoardUpdate _realTimeDashBoardUpdate, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            _realTimeDashboard = _realTimeDashBoardUpdate;

        }

        private readonly IRealTimeDashBoardUpdate _realTimeDashboard;
    }
}
