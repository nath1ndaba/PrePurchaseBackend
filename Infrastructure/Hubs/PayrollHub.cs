using BackendServices.Actions.Admin.RealTimeServices;
using BackendServices.Exceptions;
using DnsClient.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public class PayrollHub : Hub
    {
        public async Task JoinCompanyGroup(string companyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
        }

        //public async Task RequestTimeSummariesData(string companyId, DateTime startTime, DateTime endDate)
        //{
        //    try
        //    {
        //        // Fetch dashboard data based on the companyId received
        //        IAsyncEnumerable<IEnumerable<TimeSummaryWithEmployeeDetails>> timeSummaries = StreamFilteredTimeSummariesToClients(companyId, startTime, endDate);


        //        //IEnumerable<TimeSummaryWithEmployeeDetails>? results = timeSummaries.ToListAsync();
        //        //_logger.LogInformation($"Times data [{results?.Count()}] collected for company {companyId}");


        //        // Send the timeSummaries data back to the caller
        //        await Clients.Caller.SendAsync("timeSummaries", timeSummaries);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        throw new HttpResponseException(ex.Message);
        //    }
        //}



        public PayrollHub(IRealTimePayroll realTimePayroll, ILogger<DashboardUIHub> logger)
        {
            _realTimePayroll = realTimePayroll;
            _logger = logger;
        }

        public async Task RequestTimeSummariesData(string companyId, DateTime startTime, DateTime endDate)
        {
            endDate = endDate.Hour == 0 ? endDate.AddHours(23).AddMinutes(59).AddSeconds(59) : endDate;
            await foreach (IEnumerable<TimeSummaryWithEmployeeDetails>? timeSummary in _realTimePayroll.StreamFilteredTimeSummariesToClients(companyId, startTime, endDate))
            {
                _logger.LogInformation($"Times data [{timeSummary?.Count()}] collected for company {companyId}");

                // Send the timeSummaries data back to the caller
                await Clients.Caller.SendAsync("timeSummaries", timeSummary);
            }
        }

        private readonly IRealTimePayroll _realTimePayroll;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
    }


}
