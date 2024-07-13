using PrePurchase.Models;
using System;
using System.Collections.Generic;

namespace BackendServices.Actions.Admin.RealTimeServices
{
    public interface IRealTimePayroll
    {
        IAsyncEnumerable<IEnumerable<TimeSummaryWithEmployeeDetails>> StreamFilteredTimeSummariesToClients(string companyId, DateTime start, DateTime end);
    }
}
