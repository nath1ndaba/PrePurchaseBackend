using BackendServices.Models;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendServices
{
    public interface ITimeSummaryRepository : IRepository<TimeSummary>
    {
        Task<IEnumerable<TimeSummaryWithEmployeeDetails>> FindTimeSummariesForRange(string companyId, DateTime start, DateTime end, int skip = 0, int limit = 100);
        Task<IEnumerable<TimeSummary>> FindTimeSummariesForRangeForSpecificEmployees(string companyId, List<string> employessIds, DateTime start, DateTime end, int skip = 0, int limit = 100);
        Task<TimeSummary> TimeSummaryByEmployeeDetailsAndCompanyId(string employeeDetailsId, string companyId);
    }
}
