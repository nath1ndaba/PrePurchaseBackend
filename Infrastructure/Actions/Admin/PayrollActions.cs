using BackendServices;
using BackendServices.Actions;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.HistoryModels;
using PrePurchase.Models.LeaveDays;
using PrePurchase.Models.Requests;
using PrePurchase.Models.StatementsModels;
using PrePurchase.Models.Extractors;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
#nullable enable
    internal class PayrollActions : IPayrollActions
    {
        //this 
        private readonly ITimeSummaryRepository _timeSummaries;
        private readonly IRepository<EmployeeDetails> _employees;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IRepository<Company> _companies;
        private readonly IRepository<Loan> _loans;
        private readonly IRepository<LeaveStore> _leaveStore;
        private readonly IRepository<History> _histories;
        private readonly IRepository<ProcessedTimesSummary> _processedTimesSummary;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly IEmployeeActions _employeeActions;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUpdateBuilderProvider _updateBuilderProvider;
        private readonly ILogger<PayrollActions> _logger;
        private readonly ICommon _common;



        public PayrollActions(
              ITimeSummaryRepository timeSummaries
            , IRepository<EmployeeDetails> employees
            , IRepository<CompanyEmployee> companyEmployees
            , IRepository<Company> companies
            , IRepository<Loan> loans
            , IRepository<LeaveStore> leaveStore
            , IRepository<History> histories
            , IRepository<ProcessedTimesSummary> processedTimesSummary
            , IQueryBuilderProvider queryBuilderProvider
            , IEmployeeActions employeeActions
            , IUpdateBuilderProvider updateBuilderProvider
            , IDateTimeProvider dateTimeProvider
            , ILogger<PayrollActions> logger
            , ICommon common
            )
        {
            _timeSummaries = timeSummaries;
            _employees = employees;
            _companyEmployees = companyEmployees;
            _companies = companies;
            _processedTimesSummary = processedTimesSummary;
            _loans = loans;
            _leaveStore = leaveStore;
            _histories = histories;
            _queryBuilderProvider = queryBuilderProvider;
            _employeeActions = employeeActions;
            _updateBuilderProvider = updateBuilderProvider;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _common = common;
        }

        public async Task<Response> GetprocessedTimesSummaries(string role, string? companyid = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyid);

            IEnumerable<ProcessedTimesSummary> times = await _processedTimesSummary.Find(x => x.CompanyId == company.Id && x.DeletedIndicator == false);

            return new Response<IEnumerable<ProcessedTimesSummary>>(times);

        }

        public async Task<Response> GetProcessedTimesSummariesBatch(string role, string batchCode, string? companyid = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyid);

            ProcessedTimesSummary? times = await _processedTimesSummary.FindOne(x => x.CompanyId == company.Id && x.HistoryBatchCode == batchCode);

            return new Response<ProcessedTimesSummary>(times);
        }

        public async Task<Response> UndoProcessedTimesSummariesBatch(string updatedBy, string role, string batchCode, string? companyid = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyid);

            ProcessedTimesSummary? times = await _processedTimesSummary.FindOne(x => x.CompanyId == company.Id && x.HistoryBatchCode == batchCode);

            History undoneBatch = await _histories.FindOne(x => x.CompanyId == company.Id && x.IsPaid == false && x.DeletedIndicator == false && x.BatchCode == batchCode);

            foreach (TimeSummaryWithEmployeeDetails? time in times.timeSummaries)
            {
                TimeSummary exists = await _timeSummaries.FindOne(x => x.EmployeeId == time.CompanyEmployee.EmployeeId);
                if (exists != null)
                {
                    exists.StartDate = time.StartDate;
                    exists.EndDate = time.EndDate;
                    exists.Clocks.AddRange(time.Clocks);
                    exists.UpdatedDate = DateTime.UtcNow;
                    exists.UpdatedBy = ObjectId.Parse(updatedBy);
                    exists.Clocks = exists.Clocks.OrderBy(x => x.ClockIn).ToList(); //for readability of the DB
                    await _timeSummaries.Update(exists.Id.ToString(), exists);
                }
                else
                {
                    TimeSummary undoneTime = new()
                    {
                        EmployeeDetailsId = time.CompanyEmployee.Id,
                        EmployeeId = time.CompanyEmployee.EmployeeId,
                        CompanyId = time.CompanyId,
                        StartDate = time.StartDate,
                        EndDate = time.EndDate,
                        Clocks = time.Clocks,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = ObjectId.Parse(updatedBy),
                        DeletedIndicator = false
                    };
                    await _timeSummaries.Insert(undoneTime);
                }

                Loan loan = await _loans.FindOne(x => x.EmployeeSummary.EmployeeId == time.CompanyEmployee.EmployeeId);
                if (loan is not null)
                {
                    loan.LoanAmount += undoneBatch.PaySlip.PayBackLoanAsPerAgreement;
                    await _loans.Update(loan.Id.ToString(), loan);
                }
            }

            times.DeletedIndicator = true;
            await _processedTimesSummary.Update(times.Id.ToString(), times);

            undoneBatch!.DeletedIndicator = true;
            await _histories.Update(undoneBatch.Id.ToString(), undoneBatch);

            return new Response<HttpStatusCode>(HttpStatusCode.Created);
        }

        public async Task<Response> ClockEmployeeViaAdmin(string updatedBy, AdminManualClockings model, string role, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> CompanyResponse) throw new HttpResponseException(response);

            Company company = CompanyResponse.Data!;
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            CompanyEmployee thisEmployee = await _companyEmployees.FindOne(x => x.EmployeeId == model.StaffMemberId && x.CompanyId == company.Id);
            if (thisEmployee is null) throw new HttpResponseException("No Employee Found");
            EmployeeDetails userEmpID = await _employees.FindOne(x => x.EmployeeId == model.StaffMemberId);

            // not sure what the purpose for this is, when you already have a DateTime instance in the form of "model.ClockingDate".
            // be mindful when sending DateTime over the wire, especially when you need to consider the senders local date.
            //DateTime dateValue = new DateTime(model.ClockingDate.Ticks);
            model.ClockingDate = model.ClockingDate.OrderBy(c => c.ClockingDate).ToList();
            foreach (clockingdate date in model.ClockingDate)
            {
                string DayOfTheWeek = date.ClockingDate.ToString("dddd"); // this can just be `model.ClockingDate.ToString("ddd")`
                                                                          // this uses .FirstOrDefault(), so .FirstOrDefault().Value may throw `NullPointerException` so use `null-conditional operator` to access value (?.)
                                                                          // instead of .FirstOrDefault().Value, use .FirstOrDefault()?.Value
                List<EmployeeTask> PersonsDayOfTheWeek = thisEmployee.WeekDays.FirstOrDefault(x => x.Key.Contains(DayOfTheWeek)).Value;

                // again PersonsDayOfTheWeek may be null, so null check should be before getting the count and not after
                // this should be: PersonsDayOfTheWeek is null || PersonsDayOfTheWeek.Count()==0
                if (PersonsDayOfTheWeek is null || PersonsDayOfTheWeek.Count() == 0)
                    throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""{thisEmployee.Name}""  was not assigned a shift om {DayOfTheWeek}, {date.ClockingDate.ToString("yyyy-MM-dd")}. Please set shifts first. Note: days before this date were successfully added"));

                ClockData clocks = new();

                string startTime = date.ClockingDate.ToShortDateString() + " " + model.StartTime.ToString();
                string endTime = date.ClockingDate.ToShortDateString() + " " + model.EndTime.ToString();

                clocks.ClockIn = DateTime.Parse(startTime)!;
                clocks.ClockOut = DateTime.Parse(endTime)!;
                clocks.IsProcessed = false;
                clocks.IsAdminClocking = true;
                clocks.IsSickLeaveDays = false;
                clocks.IsFamilyLeaveDays = false;
                clocks.IsAnnualLeaveDays = false;
                Dictionary<ObjectId, Shift> companyShiftsMapped = company.Shifts.MapUnique(x => x.Id);
                Dictionary<ObjectId, Rate> companyRatesMapped = company.Rates.MapUnique(x => x.Id);
                clocks.Shift = companyShiftsMapped[PersonsDayOfTheWeek.FirstOrDefault()!.Shift];
                clocks.Rate = companyRatesMapped[PersonsDayOfTheWeek.FirstOrDefault()!.Rate];
                clocks.RateType = PersonsDayOfTheWeek.FirstOrDefault()!.RateType;

                // get the time summary that is recording this employee's data
                TimeSummary? employeeTimes = await _timeSummaries.FindOne(x => x.EmployeeId == model.StaffMemberId && x.CompanyId == company.Id);
                if (employeeTimes is null) /* clocking user for the first timee*/
                {
                    TimeSummary times = new()
                    {
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = ObjectId.Parse(updatedBy),
                        CreatedBy = ObjectId.Parse(updatedBy),
                        DeletedIndicator = false,

                        EmployeeDetailsId = userEmpID.Id,
                        EmployeeId = thisEmployee.EmployeeId,
                        CompanyId = company.Id,
                        StartDate = clocks.ClockIn,
                        EndDate = clocks.ClockIn,
                    };
                    times.UpdatedDate = times.CreatedDate; //making sure that update date is exactly the same as create date

                    times.Clocks.Add(clocks);

                    await _timeSummaries.Insert(times);

                }
                else /* Updating users times*/
                {
                    employeeTimes.UpdatedDate = DateTime.UtcNow;
                    employeeTimes.UpdatedBy = ObjectId.Parse(updatedBy);

                    if (employeeTimes.Clocks.Count == 0)
                        employeeTimes.StartDate = clocks.ClockIn;
                    if (clocks.ClockIn < employeeTimes.StartDate)
                        employeeTimes.StartDate = clocks.ClockIn;
                    if (clocks.ClockOut >= employeeTimes.EndDate)
                        employeeTimes.EndDate = (DateTime)clocks.ClockOut;

                    List<ClockData> exist = employeeTimes.Clocks.Where(c => c.ClockIn <= clocks.ClockIn && c.ClockOut >= clocks.ClockIn).ToList();
                    List<ClockData> existOut = employeeTimes.Clocks.Where(c => c.ClockIn <= clocks.ClockOut.Value && c.ClockOut >= clocks.ClockOut.Value).ToList();

                    if (exist.Count() > 0 || existOut.Count > 0)
                    {
                        if (model.ClockingDate.Count() > 1)
                        {
                            string text = "Please check if you are not doubling these times@  NB: Days before this date were successfully added";
                            text = text.Replace("@", "@" + Environment.NewLine);
                            throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@"Time within this range for the ""{date.ClockingDate.ToShortDateString()}"" exist. " + text));
                        }
                        else
                            throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@"Time within this range for the ""{date.ClockingDate.ToShortDateString()}"" exist. Please check if you are not doubling these times"));


                    }

                    employeeTimes!.Clocks.Add(clocks);

                    TimeSummary result = await _timeSummaries.Update(employeeTimes.Id.ToString(), employeeTimes);

                    UpdateLeaveStore(thisEmployee.EmployeeId, company.Id, updatedBy);

                }
            }
            return new Response<HttpStatusCode>(HttpStatusCode.Created);
        }

        public async Task<Response> StoreProcessedPayrollBatch(string createdBy, string updatedBy, string role, BatchRequest model, string AdminWhoProcessed, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            //find company
            if (response is not Response<Company> companyResponse) throw new HttpResponseException(response);
            Company company = companyResponse.Data!;
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            DateTimeOffset startDate = new(new DateTime(model.StartDate.Year, model.StartDate.Month, model.StartDate.Day, 0, 0, 0));
            DateTimeOffset endDate = model.EndDate.Hour == 0 ? model.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) : model.EndDate;

            //group the imployees by ids

            IEnumerable<string> employeesIds = model.PayrollEmployeesIds;
            IQueryBuilder<CompanyEmployee> queryBuilder = _queryBuilderProvider.For<CompanyEmployee>();

            //ensure the _employees belong to this company
            IQueryBuilder<CompanyEmployee> query = queryBuilder.Eq(x => x.CompanyId, ObjectId.Parse(companyId))
                .And<CompanyEmployee>(queryBuilder.In(x => x.EmployeeId, employeesIds));

            IEnumerable<CompanyEmployee> employees = await _companyEmployees.Find(query, limit: employeesIds.Count());

            if (!employees.Any()) throw new HttpResponseException($"These employees do not belongs to {companyResponse.Data!.CompanyName}!");

            if (employees.Count() != employeesIds.Count()) throw new HttpResponseException($"Some employees do not belongs to {companyResponse.Data!.CompanyName}!");

            //get the time summary from the start date to end date
            IEnumerable<TimeSummary> timeSummary = await this._timeSummaries.FindTimeSummariesForRangeForSpecificEmployees(companyId,
                        model.PayrollEmployeesIds.ToList<string>(),
                                startDate.UtcDateTime, endDate.UtcDateTime,
                                0, 100);

            if (!timeSummary.Any()) return new(HttpStatusCode.NotFound, error: "No time summary for this date range.");

            List<TimeSummary> _timeSummaries = timeSummary.ToList();
            List<TimeSummaryRecord> _timeSummariesRecord = new(_timeSummaries.Count);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //storing old times for refferal of data
            Response result = await _employeeActions.TimeSummariesForRange(companyId,
                                model.StartDate.UtcDateTime, endDate.UtcDateTime,
                                0, 100);

            if (result is not Response<IEnumerable<TimeSummaryWithEmployeeDetails>> _responseData) throw new HttpResponseException(result);

            int i = 0;
            ProcessedTimesSummary? ProcessedTimes = new ProcessedTimesSummary();
            List<string> empIds = model.PayrollEmployeesIds.OrderBy(x => x).ToList();
            IOrderedEnumerable<TimeSummaryWithEmployeeDetails> data = _responseData.Data!.OrderBy(x => x.CompanyEmployee.EmployeeId);
            ProcessedTimes.timeSummaries.Clear();
            foreach (TimeSummaryWithEmployeeDetails tswed in data)
            {
                if (empIds.Count > i)
                {
                    if (tswed.CompanyEmployee.EmployeeId == empIds[i])
                    {
                        ProcessedTimes.timeSummaries.Add(tswed);
                        i++;
                    }
                }
            }
            //Generate BatchCode for future refecence
            string? batchCode = await this._histories.CreateBatchCode(_queryBuilderProvider);
            ProcessedTimes.HistoryBatchCode = batchCode;
            ProcessedTimes.MyBatch = model.StartDate.UtcDateTime.ToString("yyMMMdd") + "-" + endDate.UtcDateTime.ToString("yyMMMdd");
            ProcessedTimes.CompanyId = company.Id;
            ProcessedTimes.CreatedBy = ObjectId.Parse(createdBy);
            ProcessedTimes.UpdatedBy = ObjectId.Parse(updatedBy);
            ProcessedTimes.CreatedDate = DateTime.UtcNow;
            ProcessedTimes.UpdatedDate = DateTime.UtcNow;
            ProcessedTimes.DeletedIndicator = false;

            await _processedTimesSummary.Insert(ProcessedTimes);
            //storing old times for referal of data
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            List<KeyValuePair<IQueryBuilder<TimeSummary>, IUpdateBuilder<TimeSummary>>> updates = new(_timeSummaries.Count);

            foreach (TimeSummary _timeSummary in _timeSummaries)
            {
                TimeSummaryRecord? _record = _timeSummary.GetClockDataForRange(model.StartDate.UtcDateTime, endDate.UtcDateTime);

                if (!_record.Clocks.Any())
                    continue;

                decimal? dailyBonusRate = company.Rates?.Where(x => x.NameOfPosition == _record.Clocks.FirstOrDefault()!.Rate.NameOfPosition).FirstOrDefault()?.DailyBonus ?? 0;
                int numberOfDaysWorked = _record.Clocks.Where(x => x.IsSickLeaveDays is false)
                                                       .Where(x => x.IsAnnualLeaveDays is false)
                                                       .Where(x => x.IsFamilyLeaveDays is false)
                                                       .GroupBy(x => (x.ClockIn.Year, x.ClockIn.DayOfYear))
                                                       .Count();

                _record.DailyBonusDays = numberOfDaysWorked * dailyBonusRate.Value;

                //Leave days taken
                _record.TheLeave.AnnualLeaveTaken = _record.Clocks.Where(x => x.IsAnnualLeaveDays is true).GroupBy(x => (x.ClockIn.Year, x.ClockIn.DayOfYear)).Count();
                _record.TheLeave.SickLeaveTaken = _record.Clocks.Where(x => x.IsSickLeaveDays is true).GroupBy(x => (x.ClockIn.Year, x.ClockIn.DayOfYear)).Count();
                _record.TheLeave.FamilyLeaveTaken = _record.Clocks.Where(x => x.IsFamilyLeaveDays is true).GroupBy(x => (x.ClockIn.Year, x.ClockIn.DayOfYear)).Count();


                //Storing Leave
                LeaveStore? LeaveStore = await _leaveStore.FindOne(x => x.EmployeeId == _record.EmployeeId) ?? new LeaveStore();
                _record.TheLeave.AnnualLeaveDescription = "Annual";
                _record.TheLeave.FamilyLeaveLeaveDescription = "Family";
                _record.TheLeave.SickLeaveDescription = "Sick";

                //Balance
                _record.TheLeave.AnnualLeaveEndKnownAsBalance = LeaveStore.AnnualLeaveDays - _record.TheLeave.AnnualLeaveTaken;
                _record.TheLeave.SickLeaveEndKnownAsBalance = LeaveStore.SickLeaveDays - _record.TheLeave.SickLeaveTaken;
                _record.TheLeave.FamilyLeaveEndKnownAsBalance = LeaveStore.FamilyLeaveDays - _record.TheLeave.FamilyLeaveTaken;


                //Leave Accrued
                _record.TheLeave.AnnualLeaveAccrued = numberOfDaysWorked * decimal.Divide(15, 260);
                _record.TheLeave.SickLeaveAccrued = numberOfDaysWorked * decimal.Divide(1, 26);
                _record.TheLeave.FamilyLeaveAccrued = 0b0;

                ///Leave Start   
                _record.TheLeave.AnnualLeaveStart = LeaveStore.AnnualLeaveDays - _record.TheLeave.AnnualLeaveAccrued;
                _record.TheLeave.SickLeaveStart = LeaveStore.SickLeaveDays - _record.TheLeave.SickLeaveAccrued;
                _record.TheLeave.FamilyLeaveStart = LeaveStore.FamilyLeaveDays - _record.TheLeave.FamilyLeaveAccrued;




                if (company.IsLoanActive)
                {
                    _record.PayBackLoanAsPerAgreement = await GetLoanBalanceToPay(_record.EmployeeId, model.BatchType);
                }

                _timeSummariesRecord.Add(_record);

                IUpdateBuilder<TimeSummary> _updateBuilder = _updateBuilderProvider.For<TimeSummary>();
                IQueryBuilder<TimeSummary> _queryBuilder = _queryBuilderProvider.For<TimeSummary>();

                ClockData? earliestClock = _timeSummary.Clocks.MinBy(x => x.ClockIn);
                DateTime newStartDate = _dateTimeProvider.Now;
                if (earliestClock is not null)
                {
                    newStartDate = earliestClock.ClockIn;
                }

                _updateBuilder = _updateBuilder
                    .Set(x => x.StartDate, newStartDate)
                    .Set(x => x.Clocks, _timeSummary.Clocks);

                IQueryBuilder<TimeSummary> _query = _queryBuilder.Eq(x => x.Id, _timeSummary.Id);
                updates.Add(new(_query, _updateBuilder));
            }

            if (!_timeSummariesRecord.Any()) return new(HttpStatusCode.NotFound, error: "No time summary records for this date range.");

            IEnumerable<History> _histories = _timeSummariesRecord.Select(x =>
            {
                History _history = new History()
                {
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = ObjectId.Parse(createdBy),
                    UpdatedBy = ObjectId.Parse(updatedBy),
                    DeletedIndicator = false,
                    BatchCode = batchCode,

                }.GeneratePaySlip(x);
                _history.UpdatedDate = _history.CreatedDate; //making sure that create and update are exactly the same

                decimal totalAmount = _history.PaySlip.TotalAmount;

                decimal uifDeduction = totalAmount.GetUIFDeduction();
                decimal taxDeduction = (totalAmount - uifDeduction).GetTaxDeduction();

                if (taxDeduction > 0)
                    _history.PaySlip.WorkPeriod.Add(new PaymentInfo { Amount = taxDeduction, Type = PaymentInfo.PaymentType.TAX });

                if (uifDeduction > 0)
                    _history.PaySlip.WorkPeriod.Add(new PaymentInfo { Amount = uifDeduction, Type = PaymentInfo.PaymentType.UIF });

                return _history;
            });

            await this._histories.Insert(_histories);

            //once we have generated the history from range
            //we got to timesummary and remove it

            await this._timeSummaries.Update(updates);

            return new Response<IEnumerable<History>>(_histories, HttpStatusCode.Created);
        }

        public async Task<Response> UpdateProcessedPayrollBatch(string updatedBy, string role, string BatchCode, List<AdjustedValuesOnPay> valuesOnPay, string AdminWhoUpdated, string? companyId = null)
        {
            //TODO: Currently only updating valuesOnPay, To find a way or updating data that gets passed to pitch
            Company company = await _common.ValidateOwner<Company>(role, companyId);


            IEnumerable<History> HistoriesToBeAdjusted = await _histories.Find(x => x.BatchCode == BatchCode && x.CompanyId == company.Id);
            HistoriesToBeAdjusted = HistoriesToBeAdjusted.OrderByDescending(x => x.Id).ToList();
            valuesOnPay = valuesOnPay.OrderByDescending(x => x.HistoryId).ToList();

            if (HistoriesToBeAdjusted.Any(x => x.PaySlip.AdjustedValuesOnPay is null))
            {
                foreach (History history in HistoriesToBeAdjusted)
                {
                    history.PaySlip.AdjustedValuesOnPay = valuesOnPay.FirstOrDefault(x => x.HistoryId == history.Id.ToString());

                    history.UpdatedDate = DateTime.UtcNow;
                    history.UpdatedBy = ObjectId.Parse(updatedBy);
                    await _histories.Update(history.Id.ToString(), history);
                }
            }
            else
            {
                //update only the NOT original Values
                foreach (History history in HistoriesToBeAdjusted)
                {
                    history.PaySlip.AdjustedValuesOnPay.AdjustedBonus = valuesOnPay.FirstOrDefault(x => x.HistoryId == history.Id.ToString())!.AdjustedBonus;
                    history.PaySlip.AdjustedValuesOnPay.AdjustedLoan = valuesOnPay.FirstOrDefault(x => x.HistoryId == history.Id.ToString())!.AdjustedLoan;
                    history.PaySlip.AdjustedValuesOnPay.AdjustedNet = valuesOnPay.FirstOrDefault(x => x.HistoryId == history.Id.ToString())!.AdjustedNet;
                    history.PaySlip.AdjustedValuesOnPay.AccessFunds = valuesOnPay.FirstOrDefault(x => x.HistoryId == history.Id.ToString())!.AccessFunds;

                    history.UpdatedDate = DateTime.UtcNow;
                    history.UpdatedBy = ObjectId.Parse(updatedBy);
                    await _histories.Update(history.Id.ToString(), history);
                }
            }

            return await GetProcessedPayrollBatch(role, companyId);
        }

        public async Task<Response> GetProcessedPayrollBatch(string role, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            Company company = companyResponse.Data!;
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            PayrollData payrollData = new();

            //TODO: add paramaters so you can page or skip and limit
            IEnumerable<History> payrollBatch = await _histories.Find(x => x.CompanyId == company.Id && x.IsPaid == false && x.DeletedIndicator == false);


            List<string> employeesIds = payrollBatch.Select(x => x.EmployeeId).ToList();
            IQueryBuilder<CompanyEmployee> queryBuilder = _queryBuilderProvider.For<CompanyEmployee>();

            IQueryBuilder<CompanyEmployee> query = queryBuilder.Eq(x => x.CompanyId, ObjectId.Parse(companyId))
                .And<CompanyEmployee>(queryBuilder.In(x => x.EmployeeId, employeesIds));

            IEnumerable<CompanyEmployee> employeesEnum = await _companyEmployees.Find(query, limit: employeesIds.Count);

            Dictionary<string, CompanyEmployee> employeeDictionary = employeesEnum.ToDictionary(x => x.EmployeeId);

            payrollBatch = payrollBatch.Select(x =>
            {
                CompanyEmployee employee = employeeDictionary[x.EmployeeId];
                x.Name = employee?.Name;
                x.Surname = employee?.Surname;
                return x;
            }).ToList();

            return new Response<IEnumerable<History>>(payrollBatch);
        }

        public async Task<Response> GetPayrolBatchHistory(string role, string? companyId = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyId);


            //TO-Do: add paramaters so you can page or skip and limit
            IEnumerable<History> payrollBatch = await _histories.Find(x => x.CompanyId == company.Id, limit: 100);

            IEnumerable<string> employeesIds = payrollBatch.Select(x => x.EmployeeId);
            IQueryBuilder<CompanyEmployee> queryBuilder = _queryBuilderProvider.For<CompanyEmployee>();

            IQueryBuilder<CompanyEmployee> query = queryBuilder.Eq(x => x.CompanyId, ObjectId.Parse(companyId))
                .And<CompanyEmployee>(queryBuilder.In(x => x.EmployeeId, employeesIds));

            IEnumerable<CompanyEmployee> employees = await _companyEmployees.Find(query, limit: employeesIds.Count());

            IEnumerable<BatchHistory> histories = payrollBatch.Select(x =>
            {
                CompanyEmployee? employee = employees.FirstOrDefault(y => y.EmployeeId == x.EmployeeId);
                x.Name = employee?.Name;
                x.Surname = employee?.Surname;

                return BatchHistory.FromHistory(x);
            });
            Response<IEnumerable<BatchHistory>> results = new(histories);
            return results;
        }


        private List<ClockData> AmendedListOfClocks = new();
        public async Task<Response> AmendClockings(string updatedBy, List<AmendClocks> orderedUpdatedNewTimes, string employeeId, string role, string? companyId = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyId);

            AmendedListOfClocks.Clear();

            // get the time summary that is recording this employee's data
            TimeSummary employeeTimes = await _timeSummaries.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == company.Id);
            employeeTimes.UpdatedDate = DateTime.UtcNow;
            employeeTimes.UpdatedBy = ObjectId.Parse(updatedBy);

            List<ClockData> orderedOldTimes = employeeTimes.Clocks.OrderBy(c => c.ClockIn).ToList();

            //var startRangeDate = orderedNUpdatedNewTimes.FirstOrDefault()!.ClockIn;
            //var endRangeDate = orderedNUpdatedNewTimes.LastOrDefault()!.ClockIn;
            //var StoreNewDataStart = orderedOldTimes.Where(c=>c.ClockIn < startRangeDate).ToList();
            //var StoreNewDataEnd = orderedOldTimes.Where(c=>c.ClockIn > endRangeDate).ToList();

            //StoreNewDataStart.AddRange(StoreNewDataEnd);

            //Getting time for each user for that range and then order it
            //orderedOldTimes = employeeTimes.Clocks.Where(c => c.ClockIn >= orderedNUpdatedNewTimes.FirstOrDefault()!.ClockIn).ToList();  
            //orderedOldTimes = orderedOldTimes.Where(c=>c.ClockIn <=orderedNUpdatedNewTimes.LastOrDefault()!.ClockIn).ToList();

            /*
             * old times    New Times
             * 3 sept       -------
             * 4 sept       4 sept*  start date
             * 5 sept       5 sept*
             * 6 sept       6 sept*
             * 7 sept       7 sept*
             * 8 sept       8 sept*   end date
             * 9 sept       -------
             * 
             * 

             */

            AmendClocksRecursively(orderedOldTimes, orderedUpdatedNewTimes, 0, 0);
            //int i = 0;
            //while (AmendedListOfClocks.FirstOrDefault()!.ClockIn <= orderedOldTimes.FirstOrDefault()!.ClockIn)
            //{

            //}   
            //while (AmendedListOfClocks.FirstOrDefault()!.ClockIn > orderedOldTimes.LastOrDefault()!.ClockIn)
            //{ 

            //}

            employeeTimes.Clocks = AmendedListOfClocks;
            await _timeSummaries.Update(employeeTimes.Id.ToString(), employeeTimes);

            return new Response(HttpStatusCode.OK);
        }

        private readonly List<ClockData> OverrideListOfClocks = new();
        public async Task<Response> OverrideClockings(string updatedBy, string employeeId, string role, string? companyId = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyId);


            // get the time summary that is recording this employee's data
            TimeSummary? employeeTimes = await _timeSummaries.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == company.Id);

            if (employeeTimes is null) throw new HttpResponseException($"Employee with ID({employeeId}) has no data to Update");

            employeeTimes.UpdatedDate = DateTime.UtcNow;
            employeeTimes.UpdatedBy = ObjectId.Parse(updatedBy);

            OverrideListOfClocks.Clear();
            List<ClockData> orderedTimes = employeeTimes.Clocks.OrderBy(c => c.ClockIn).ToList();
            OverrideClocksRecursively(orderedTimes, 0);
            employeeTimes.Clocks = OverrideListOfClocks.OrderBy(c => c.ClockIn).ToList();
            await _timeSummaries.Update(employeeTimes.Id.ToString(), employeeTimes);

            return new Response(HttpStatusCode.OK);
        }

        public async Task<Response> OverrideAllClockings(string updatedBy, string role, string? companyId = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyId);

            // get the time summary that is recording this employee's data
            IEnumerable<TimeSummary> CompanyTimes = await _timeSummaries.Find(x => x.CompanyId == company.Id);

            if (CompanyTimes is null)
                throw new HttpResponseException("Company has no data to Update");

            foreach (TimeSummary employeeTimes in CompanyTimes)
            {

                if (employeeTimes is not null)
                {
                    employeeTimes.UpdatedDate = DateTime.UtcNow;
                    employeeTimes.UpdatedBy = ObjectId.Parse(updatedBy);

                    OverrideListOfClocks.Clear();
                    List<ClockData>? orderedTimes = employeeTimes.Clocks.OrderBy(c => c.ClockIn).ToList();
                    OverrideClocksRecursively(orderedTimes, 0);
                    employeeTimes.Clocks = OverrideListOfClocks.OrderBy(c => c.ClockIn).ToList();

                    await _timeSummaries.Update(employeeTimes.Id.ToString(), employeeTimes);
                }

            }
            return new Response(HttpStatusCode.OK);
        }

        public async Task<Response> TimeSummariesByCompanyId(string role, string companyId)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyId);

            IEnumerable<TimeSummary> result = await _timeSummaries.Find(x => x.CompanyId == company.Id);
            return new Response<IEnumerable<TimeSummary>>(result);
        }

        public async Task<Response> TimeSummariesForRangeByCompanyId(string role, TimeSummariesForRangeModel model)
        {
            Company company = await _common.ValidateOwner<Company>(role, model.CompanyId);

            DateTime startDate = new DateTime(model.StartDate.Year, model.StartDate.Month, model.StartDate.Day, 0, 0, 0);
            DateTime endDate = model.EndDate.Hour == 0 ? model.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) : model.EndDate;

            Response result = await _employeeActions.TimeSummariesForRange(model.CompanyId,
                                startDate, endDate,
                                model.Skip, model.Limit);

            if (result is not Response<IEnumerable<TimeSummaryWithEmployeeDetails>> _responseData)
                throw new HttpResponseException(result);

            //only send unprocessed data
            IEnumerable<TimeSummaryWithEmployeeDetails> data = _responseData.Data!.Select(x =>
            {

                x.Clocks = x.Clocks.OrderBy(x => x.ClockIn).ToList();
                return x;
            });
            return new(HttpStatusCode.OK, data);
        }

        public async Task<Response> ApplyLeaveViaAdmin(string createdBy, string updatedBy, RequestLeaveModel model, string employeeId, string companyId, string role)
        {
            Response response = await GetCompany(role, companyId);

            if (model is null) throw new HttpResponseException($"{nameof(model)} can not be null");
            if (model.DaysToTake <= 0) throw new HttpResponseException($"Please porvide days more than zero");

            Company company = await _common.ValidateOwner<Company>(role, companyId);

            Response leave = await _employeeActions.RequestLeave(createdBy, updatedBy, LeaveStatus.Accepted, model, employeeId, companyId);
            if (leave.StatusCode is HttpStatusCode.Created)
            {
                LeaveStore currentStore = await _leaveStore.FindOne(x => x.CompanyId == ObjectId.Parse(companyId) && x.EmployeeId.ToLower().Contains(employeeId.Trim()));
                currentStore ??= new LeaveStore();
                switch (model.TypeOfLeave)
                {
                    case TypeOfLeave.Annual:
                        currentStore.AnnualLeaveDays -= model.DaysToTake;
                        break;
                    case TypeOfLeave.Sick:
                        currentStore.SickLeaveDays -= model.DaysToTake;
                        break;
                    case TypeOfLeave.Parental:
                        currentStore.FamilyLeaveDays -= model.DaysToTake;
                        break;
                    case TypeOfLeave.Unpaid:
                        currentStore.FamilyLeaveDays -= model.DaysToTake;
                        break;
                    case TypeOfLeave.Martenity:
                        currentStore.FamilyLeaveDays -= model.DaysToTake;
                        break;
                    default:
                        break;
                }
                currentStore.UpdatedDate = DateTime.UtcNow;
                currentStore.UpdatedBy = ObjectId.Parse(updatedBy);

                TimeSummary? times = await _timeSummaries.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == ObjectId.Parse(companyId));
                if (times is null) throw new HttpResponseException("Leave can not be assigned to someone who has never worked for the company");



                //get companyId so i can know the position of this specific employee
                CompanyEmployee? comEmployee = await _companyEmployees.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLower().Contains(employeeId.Trim()));
                if (comEmployee is null) throw new HttpResponseException("Something wrong with user's info");

                leaveClock = times.Clocks;
                if (times.EndDate < model.LeaveEndDate)
                    times.EndDate = model.LeaveEndDate!; // update end date

                Rate userRates = company.Rates.FirstOrDefault(x => x.NameOfPosition == comEmployee.Position)!;
                Shift userShift = company.Shifts.FirstOrDefault()!;

                AddLeaveViaAdminToClocks(times, model, userShift, userRates, 0);


                await _timeSummaries.Update(times.Id.ToString(), Summaries);
                await _leaveStore.Update(currentStore.Id.ToString(), currentStore);

            }

            return new Response(HttpStatusCode.Created, message: "Leave Implimented fully😅");

        }

        private int jumpAfterEvery5Days = 5;
        private List<ClockData> leaveClock = new();
        private TimeSummary Summaries = new();
        private void AddLeaveViaAdminToClocks(TimeSummary times, RequestLeaveModel model, Shift userShift, Rate userRates, int len)
        {
            if (len == model.DaysToTake)
                return;

            if (len == jumpAfterEvery5Days)
            {
                len += 2; //omit 2 days to as an assumption of offdays
                jumpAfterEvery5Days += 6;//get ready for another 5 days
            }
            else
            {
                string? startTime = model.LeaveStartDate.AddDays(len).ToShortDateString() + " " + userShift.ShiftStartTime.ToString();
                string? endTime = model.LeaveStartDate.AddDays(len).ToShortDateString() + " " + userShift.ShiftEndTime.ToString();
                DateTime start = DateTime.Parse(startTime)!.AddMinutes(-120);
                DateTime end = DateTime.Parse(endTime)!.AddMinutes(-120);

                if (times.Clocks.Count == 0)
                    times.StartDate = start;
                if (start < times.StartDate)
                    times.StartDate = start;
                if (end >= times.EndDate)
                    times.EndDate = end;

                ClockData ClockDataToContainLeave = new()
                {
                    ClockIn = start,
                    ClockOut = end,
                    Rate = userRates,
                    Shift = userShift,

                    RateType = RateType.Standard,
                    IsAdminClocking = true,
                    IsProcessed = false
                };

                switch (model.TypeOfLeave)
                {
                    case TypeOfLeave.Sick:
                        ClockDataToContainLeave.IsSickLeaveDays = true;
                        break;
                    case TypeOfLeave.Annual:
                        ClockDataToContainLeave.IsAnnualLeaveDays = true;
                        break;
                    default:
                        ClockDataToContainLeave.IsFamilyLeaveDays = true;
                        break;
                }

                leaveClock.Add(ClockDataToContainLeave);
                times.Clocks = leaveClock;
                Summaries = times;
            }
            AddLeaveViaAdminToClocks(times, model, userShift, userRates, len + 1);
        }

        private void AddLeaveToClocks(TimeSummary times, QueryLeaveModel model, Shift userShift, Rate userRates, int len)
        {
            if (len == model.DaysToTake)
                return;

            if (len == jumpAfterEvery5Days)
            {
                len = len + 2; //omit 2 days to as an assumption of offdays
                jumpAfterEvery5Days = jumpAfterEvery5Days + 6;//get ready for another 5 days
            }
            else
            {
                string? startTime = model.LeaveStartDate!.Value.AddDays(len).ToShortDateString() + " " + userShift.ShiftStartTime.ToString();
                string? endTime = model.LeaveStartDate!.Value.AddDays(len).ToShortDateString() + " " + userShift.ShiftEndTime.ToString();
                DateTime start = DateTime.Parse(startTime)!.AddMinutes(-120);
                DateTime end = DateTime.Parse(endTime)!.AddMinutes(-120);


                if (times.Clocks.Count() == 0)
                    times.StartDate = start;
                if (start < times.StartDate)
                    times.StartDate = start;
                if (end >= times.EndDate)
                    times.EndDate = end;

                ClockData ClockDataToContainLeave = new();
                ClockDataToContainLeave.ClockIn = start;
                ClockDataToContainLeave.ClockOut = end;
                ClockDataToContainLeave.Rate = userRates;
                ClockDataToContainLeave.Shift = userShift;
                ClockDataToContainLeave.RateType = RateType.Standard;
                ClockDataToContainLeave.IsAdminClocking = true;
                ClockDataToContainLeave.IsProcessed = false;

                switch (model.TypeOfLeave)
                {
                    case TypeOfLeave.Sick:
                        ClockDataToContainLeave.IsSickLeaveDays = true;
                        break;
                    case TypeOfLeave.Annual:
                        ClockDataToContainLeave.IsAnnualLeaveDays = true;
                        break;
                    default:
                        ClockDataToContainLeave.IsFamilyLeaveDays = true;
                        break;
                }

                leaveClock.Add(ClockDataToContainLeave);
                times.Clocks = leaveClock;
                Summaries = times;
            }
            AddLeaveToClocks(times, model, userShift, userRates, len + 1);
        }

#nullable enable
        public async Task<Response> GetCompany(string role, string? id = null)
        {
            async Task<Company> Data()
                => await _companies.FindById(id);

#nullable disable
            Response response;

            switch (role)
            {
                case AuthRoles.Manager:
                    if (id is null)
                        response = new(error: "Company id is not specified!");
                    else
                        response = new Response<Company>() { Data = await Data() };
                    break;

                case AuthRoles.Owner:
                    response = new Response<Company>() { Data = await Data() };
                    break;
                default:
                    response = new Response(HttpStatusCode.Unauthorized, error: "You don't have access to this resource!");
                    break;
            }

            return response;

        }
#nullable enable

        private void AmendClocksRecursively(List<ClockData> existingCLocks, List<AmendClocks> newClocks, int len, int i)
        {
            if (existingCLocks.Count == len)
            {
                return;
            }
            try
            {

                if (newClocks.Count > i)
                {
                    //allow times to be updated as many as possible BUT ALWAYS REMEMBER THE ORIGINAL TIME
                    if (existingCLocks[len].Id == newClocks[i].Id && existingCLocks[len].IsAdminClocking is false)
                    {
                        if (string.IsNullOrEmpty(newClocks[i].Id.ToString())) throw new ArgumentException($"clocks Id may not be null");
                        if (string.IsNullOrEmpty(newClocks[i].ClockIn.ToString())) throw new ArgumentException($"clock in may not be null");
                        if (string.IsNullOrEmpty(newClocks[i].ClockOut.ToString())) throw new ArgumentException($"clock out may not be null");
                        if (string.IsNullOrEmpty(newClocks[i].RateType.ToString())) throw new ArgumentException($"Rate type may not be null");

                        if (existingCLocks[len].ClockIn.Minute != newClocks[i].ClockIn.Minute &&
                            existingCLocks[len].ClockIn.Hour != newClocks[i].ClockIn.Hour &&
                            existingCLocks[len].OldClockInValue.Year.ToString() is "1")
                        {
                            existingCLocks[len].IsClockInAdjusted = true;
                            existingCLocks[len].OldClockInValue = existingCLocks[len].ClockIn;
                        }

                        if (existingCLocks[len].ClockOut!.Value.Hour != newClocks[i].ClockOut!.Value.Hour &&
                            existingCLocks[len].ClockOut!.Value.Minute != newClocks[i].ClockOut!.Value.Minute &&
                            existingCLocks[len].OldClockOutValue.Year.ToString() is "1")
                        {
                            existingCLocks[len].IsClockOutAdjusted = true;
                            existingCLocks[len].OldClockOutValue = existingCLocks[len].ClockOut!.Value;
                        }

                        existingCLocks[len].ClockIn = newClocks[i].ClockIn.AddMinutes(-120);
                        existingCLocks[len].ClockOut = newClocks[i].ClockOut!.Value.AddMinutes(-120);
                        existingCLocks[len].RateType = newClocks[i].RateType;
                        AmendedListOfClocks.Add(existingCLocks[len]);
                        i++;
                    }

                    //No need to keep track of old time if the first time was manualy added via admin. always falsifiable!
                    else if (existingCLocks[len].Id == newClocks[i].Id && existingCLocks[len].IsAdminClocking is true)
                    {
                        if (string.IsNullOrEmpty(newClocks[i].Id.ToString())) throw new ArgumentException($"clocks Id may not be null");
                        if (string.IsNullOrEmpty(newClocks[i].ClockIn.ToString())) throw new ArgumentException($"clock in may not be null");
                        if (string.IsNullOrEmpty(newClocks[i].ClockOut.ToString())) throw new ArgumentException($"clock out may not be null");
                        if (string.IsNullOrEmpty(newClocks[i].RateType.ToString())) throw new ArgumentException($"Rate type may not be null");

                        if (existingCLocks[len].ClockIn != newClocks[i].ClockIn)
                        {
                            existingCLocks[len].IsClockInAdjusted = true;
                            existingCLocks[len].OldClockInValue = existingCLocks[len].ClockIn;
                        }

                        if (existingCLocks[len].ClockOut != newClocks[i].ClockOut)
                        {
                            existingCLocks[len].IsClockOutAdjusted = true;
                            existingCLocks[len].OldClockOutValue = existingCLocks[len].ClockOut!.Value;

                        }

                        existingCLocks[len].ClockIn = newClocks[i].ClockIn.AddMinutes(-120);
                        existingCLocks[len].ClockOut = newClocks[i].ClockOut!.Value.AddMinutes(-120);
                        existingCLocks[len].RateType = newClocks[i].RateType;
                        AmendedListOfClocks.Add(existingCLocks[len]);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally { }
            //else
            //throw new HttpResponseException("Times not grouped properly");
            //do nothing n proceed

            AmendClocksRecursively(existingCLocks, newClocks, len + 1, i);
        }

        private void OverrideClocksRecursively(List<ClockData> existingCLocks, int len)
        {
            if (existingCLocks.Count == len)
            {
                return;
            }

            if (existingCLocks[len].ClockOut is null)
            {
                string? referenceDate = existingCLocks[len].ClockIn!.Date.ToShortDateString() + " " + existingCLocks[len].Shift.ShiftEndTime.ToString();




                existingCLocks[len].ClockOut = DateTime.Parse(referenceDate).AddMinutes(-120);
                existingCLocks[len].IsClockOutAdjusted = true;

                OverrideListOfClocks.Add(existingCLocks[len]);

            }
            else
                OverrideListOfClocks.Add(existingCLocks[len]);

            OverrideClocksRecursively(existingCLocks, len + 1);
        }

        private async void UpdateLeaveStore(string employeeId, ObjectId companyId, string updatedBy)
        {
            LeaveStore leave = await _leaveStore.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == companyId);
            if (leave is not null)
            {
                leave.UpdatedDate = DateTime.UtcNow;
                leave.UpdatedBy = ObjectId.Parse(updatedBy);

                leave.AnnualLeaveDays += decimal.Divide(15, 260);// * 0.0576923076923077;
                leave.SickLeaveDays += decimal.Divide(1, 26); // * 0.0384615384615385)
                leave.TimeStamp = _dateTimeProvider.Now.Date;

                await _leaveStore.Update(leave.Id.ToString(), leave);
            }

            else if (leave is null)
            {
                CompanyEmployee? user = await _companyEmployees.FindOne(x => x.EmployeeId.ToLower().Contains(employeeId.Trim()));
                leave = new LeaveStore
                {
                    CreatedDate = _dateTimeProvider.Now.Date,
                    CreatedBy = user.Id,
                    UpdatedDate = _dateTimeProvider.Now.Date,
                    UpdatedBy = user.Id,
                    DeletedIndicator = false,

                    AnnualLeaveDays = decimal.Divide(15, 260),// * 0.0576923076923077;
                    SickLeaveDays = decimal.Divide(1, 26), // * 0.0384615384615385)
                    FamilyLeaveDays = 3,
                    Name = user.Name,
                    Surname = user.Surname,
                    CompanyId = companyId,
                    EmployeeId = employeeId,
                    TimeStamp = _dateTimeProvider.Now.Date
                };
                await _leaveStore.Insert(leave);
            }
        }

        private async Task<decimal> GetLoanBalanceToPay(string employeeId, BatchType batchType)
        {
            List<Loan> loans = await _loans.Find(x => x.EmployeeSummary.EmployeeId == employeeId && x.LoanStatus == LoanStatus.Accepted).ToListAsync();
            decimal payBackLoanAsPerAgreement = 0;
            if (loans.Count > 0)
            {
                try
                {
                    decimal startAmount = loans.Sum(x => x.LoanAmount);
                    decimal paidBackAmount = loans.Sum(x => x.AmountPayed);

                    decimal loanBalance = startAmount - paidBackAmount;
                    TimeSpan stamp = TimeSpan.Zero;
                    foreach (Loan loan in loans)
                    {
                        stamp = loan.TimeStamp.AddMonths(decimal.ToInt32(loan.LoanDurationInMonths)) - DateTime.UtcNow;
                    }

                    int loanDuration = stamp.Days > 0 ? stamp.Days : -stamp.Days;

                    Loan? recentLoan = loans.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                    double remainingTimesToPay = 0;

                    switch (batchType)
                    {
                        case BatchType.Daily:
                            remainingTimesToPay = loanDuration / 1;
                            break;
                        case BatchType.Weekly:
                            remainingTimesToPay = loanDuration / 7;
                            break;
                        case BatchType.ForthNight:
                            remainingTimesToPay = loanDuration / 15;
                            break;
                        case BatchType.Monthly:
                            remainingTimesToPay = loanDuration / 30;
                            break;
                    }
                    remainingTimesToPay = remainingTimesToPay > 0 ? remainingTimesToPay : -remainingTimesToPay;

                    if (remainingTimesToPay > 0)
                    {
                        payBackLoanAsPerAgreement = loanBalance / (decimal)remainingTimesToPay;
                    }

                    //overdue
                    if (loanDuration <= 0)
                    {
                        payBackLoanAsPerAgreement = loanBalance;
                    }

                    decimal remainingBalance = loanBalance - payBackLoanAsPerAgreement;

                    recentLoan!.AmountPayed += payBackLoanAsPerAgreement;
                    recentLoan.LastPayment = payBackLoanAsPerAgreement;
                    recentLoan.LastPaymentDate = DateTime.UtcNow;

                    if (loanBalance <= recentLoan.AmountPayed)
                    {
                        recentLoan.LoanStatus = LoanStatus.PaidOff;
                    }
                    await _loans.Update(recentLoan.Id.ToString(), recentLoan);
                    return payBackLoanAsPerAgreement;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }
            return payBackLoanAsPerAgreement;
        }
    }
}
