using BackendServices.Actions.Admin.RealTimeServices;
using BackendServices.Exceptions;
using BackendServices.Models.RealTime;
using DnsClient.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public class DashboardUIHub : Hub
    {
        public async Task JoinCompanyGroup(string companyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
        }

        public Task SendDashboardToCompany(string companyId, DashboardUIData dashboardUIData)
        {
            return Clients.Group(companyId).SendAsync("dashboardUI", dashboardUIData);
        }


        public async Task RequestDashboardData(string companyId)
        {
            try
            {
                // Fetch dashboard data based on the companyId received
                DashboardUIData dashboard = await FetchDashboardData(companyId);
                _logger.LogInformation($"dashboard data collected for company {companyId}");
                // Send the dashboard data back to the caller
                await Clients.Caller.SendAsync("dashboardUI", dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new HttpResponseException(ex.Message);
            }
        }


        public DashboardUIHub(IRealTimeDashBoardUpdate realTimeDashBoardUpdate, ILogger<DashboardUIHub> logger)
        {
            _realTimeDashBoardUpdate = realTimeDashBoardUpdate;
            _logger = logger;
        }

        private async Task<DashboardUIData> FetchDashboardData(string companyId)
        {
            try
            {
                DashboardUIData dashboardData = await _realTimeDashBoardUpdate.GetDashboardUIData(companyId);

                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new HttpResponseException(ex.Message);
            }
        }

        private readonly IRealTimeDashBoardUpdate _realTimeDashBoardUpdate;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
    }


}
