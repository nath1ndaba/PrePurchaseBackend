using BackendServices;
using BackendServices.Actions.Admin.RealTimeServices;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin.RealTime
{
    internal class RealTimePayroll : BaseRepository<TimeSummary>, IRealTimePayroll
    {
        private readonly ILogger<RealTimeDashBoardUpdate> _logger;
        private readonly IRepository<CompanyEmployee> _companyEmployee;
        private readonly ITimeSummaryRepository _timeSummaryRepository;
        public RealTimePayroll(RepositoryManager repositoryManager, IRepository<CompanyEmployee> companyEmployee, ILogger<RealTimeDashBoardUpdate> logger, ITimeSummaryRepository timeSummaryRepository) : base(repositoryManager)
        {
            _logger = logger;
            _companyEmployee = companyEmployee;
            _timeSummaryRepository = timeSummaryRepository;
        }


        public async IAsyncEnumerable<IEnumerable<TimeSummaryWithEmployeeDetails>> StreamFilteredTimeSummariesToClients(string companyId, DateTime start, DateTime end)
        {
            int skip = 0;
            int batchSize = 10;
            while (true)
            {
                IEnumerable<TimeSummaryWithEmployeeDetails> timeSummaries = await _timeSummaryRepository.FindTimeSummariesForRange(companyId, start, end, skip, batchSize);

                yield return timeSummaries;

                if (timeSummaries.Count() == 0)
                {
                    break; // No more data to fetch
                }

                skip += batchSize;
            }
        }


        private async Task<IEnumerable<CompanyEmployee>> GetEmployeesForIds(ObjectId companyId, List<string> employeeIds)
        {
            return await _companyEmployee
                .Find(x => x.CompanyId == companyId && employeeIds.Contains(x.EmployeeId))
                .ToListAsync();
        }
    }
}
