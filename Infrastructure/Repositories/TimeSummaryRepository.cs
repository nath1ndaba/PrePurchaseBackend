using BackendServices;
using BackendServices.Exceptions;
using BackendServices.Models;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    internal class TimeSummaryRepository : BaseRepository<TimeSummary>, ITimeSummaryRepository
    {
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly ILogger<TimeSummary> logger;

        public TimeSummaryRepository(RepositoryManager repositoryManager, IQueryBuilderProvider queryBuilderProvider, ILogger<TimeSummary> logger) : base(repositoryManager)
        {
            _companyEmployees = repositoryManager.GetRepository<CompanyEmployee>();
            _queryBuilderProvider = queryBuilderProvider;
            this.logger = logger;
        }

        public async Task<IEnumerable<TimeSummary>> FindTimeSummariesForRangeForSpecificEmployeess(string companyId, List<string> employeeIds, DateTime start, DateTime end, int skip = 0, int limit = 100)
        {
            FilterDefinition<TimeSummary> filter = Builders<TimeSummary>.Filter.And(
                Builders<TimeSummary>.Filter.Eq(x => x.CompanyId, ObjectId.Parse(companyId)),
                Builders<TimeSummary>.Filter.In(x => x.EmployeeId, employeeIds),
                Builders<TimeSummary>.Filter.Gte(x => x.StartDate, start),
                Builders<TimeSummary>.Filter.Lte(x => x.EndDate, end),
                Builders<TimeSummary>.Filter.Where(x => x.Clocks.Any(clock => clock.TotalHours > 0)) // Filtering directly in the query
            );

            FindOptions<TimeSummary> options = new()
            {
                Skip = skip,
                Limit = limit
            };

            IAsyncCursor<TimeSummary> cursor = await _collection.FindAsync(filter, options);
            List<TimeSummary> timeSummaries = await cursor.ToListAsync();

            return timeSummaries;
        }

        public async Task<IEnumerable<TimeSummary>> FindTimeSummariesForRangeForSpecificEmployees(string companyId, List<string> employeeIds, DateTime start, DateTime end, int skip = 0, int limit = 100)
        {
            try
            {
                var filter = Builders<TimeSummary>
                    .Filter.And(
                        Builders<TimeSummary>.Filter.Eq(x => x.CompanyId, ObjectId.Parse(companyId)),
                        Builders<TimeSummary>.Filter.In(x => x.EmployeeId, employeeIds),
                        Builders<TimeSummary>.Filter.Or(
                            Builders<TimeSummary>.Filter.Gte(x => x.StartDate, start) /*& Builders<TimeSummary>.Filter.Lte(x => x.StartDate, end)*/,
                            /*Builders<TimeSummary>.Filter.Gte(x => x.EndDate, start) &*/ Builders<TimeSummary>.Filter.Lte(x => x.EndDate, end)
                        )
                    );

                // Use asynchronous Find method
                List<TimeSummary> timeSummaries = await _collection.Find(filter)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync();

                // Use Select to perform the transformation within the query
                IEnumerable<TimeSummary> newData = timeSummaries
                    .Select(ts => ts.WithClocksInRange(start, end));

                return newData;
            }
            catch (Exception ex)
            {
                // Log the error and throw an HttpResponseException
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: ex.Message));
            }
        }

        public async Task<IEnumerable<TimeSummaryWithEmployeeDetails>> FindTimeSummariesForRange(string companyId, DateTime start, DateTime end, int skip = 0, int limit = 100)
        {
            ObjectId companyObjectId = ObjectId.Parse(companyId);
            try
            {

                FilterDefinition<TimeSummary> filter = Builders<TimeSummary>
                    .Filter.And(
                        Builders<TimeSummary>.Filter.Eq(x => x.CompanyId, companyObjectId),
                        Builders<TimeSummary>.Filter.Or(
                            Builders<TimeSummary>.Filter.Gte(x => x.StartDate, start) & Builders<TimeSummary>.Filter.Lte(x => x.StartDate, end),
                            Builders<TimeSummary>.Filter.Gte(x => x.EndDate, start) & Builders<TimeSummary>.Filter.Lte(x => x.EndDate, end)
                        )
                    );

                // Combine all filters using the & operator to create a compound filter
                FilterDefinition<TimeSummary> combinedFilter = Builders<TimeSummary>.Filter.And(filter);

                List<TimeSummary> timeSummaries = await _collection.Find(combinedFilter)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync();
                timeSummaries = timeSummaries.Where(x => x.Clocks.Count > 0).ToList();
                // Fetch employee details
                List<string> employeeIds = timeSummaries.Select(ts => ts.EmployeeId).Distinct().ToList();
                IEnumerable<CompanyEmployee> employees = await GetEmployeesForIds(companyObjectId, employeeIds);

                //// Construct and return the result
                IEnumerable<TimeSummaryWithEmployeeDetails> result = timeSummaries
                    .Select((ts) =>
                    {
                        CompanyEmployee? employee = employees
                            .Where(emp => emp.CompanyId == ts.CompanyId && emp.EmployeeId == ts.EmployeeId)
                            .FirstOrDefault()
                            ?? throw new ArgumentException($"employee {ts.EmployeeId} not found");
                        return ModelHelpers.From(ts.WithClocksInRange(start, end), employee);
                    });

                return result;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: ex.Message));
            }
            finally
            { }
        }

        public async Task<TimeSummary> TimeSummaryByEmployeeDetailsAndCompanyId(string employeeDetailsId, string companyId)
        {
            IAsyncCursor<TimeSummary> result = await _collection.FindAsync(x => x.EmployeeDetailsId == ObjectId.Parse(employeeDetailsId) && x.CompanyId == ObjectId.Parse(companyId));
            return result.FirstOrDefault();
        }

        private async Task<IEnumerable<CompanyEmployee>> GetEmployeesForIds(ObjectId companyId, List<string> employeeIds)
        {
            return await _companyEmployees
                .Find(x => x.CompanyId == companyId && employeeIds.Contains(x.EmployeeId))
                .ToListAsync();
        }
    }
}
