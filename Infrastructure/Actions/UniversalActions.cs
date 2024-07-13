using BackendServices;
using BackendServices.Actions;
using BackendServices.Actions.Admin.RealTimeServices;
using BackendServices.Exceptions;
using BackendServices.Models;
using Infrastructure.Helpers;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.LeaveDays;
using PrePurchase.Models.StatementsModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions
{
#nullable enable
    internal class UniversalActions : IUniversalActions
    {
        //this 
        private readonly ITimeSummaryRepository _timeSummaries;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IRepository<EmployeeDetails> _employees;
        private readonly IRepository<Company> _companies;
        private readonly IRepository<User> _users;
        private readonly IRepository<Loan> _loans;
        private readonly IRepository<LeaveStore> _leaveStore;
        private readonly IRepository<Leave> _leaves;
        private readonly IRepository<History> _histories;
        private readonly IRepository<Customization> _customization;
        private readonly IRepository<Supplier> _suppliers;
        private readonly IRepository<SupplierInvoices> _suppliersInvoice;
        private readonly IRepository<ProcessedTimesSummary> _processedTimesSummary;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly IPasswordManager _passwordManager;
        private readonly IEmployeeIdGenerator _idGenerator;
        private readonly IEmployeeActions _employeeActions;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUpdateBuilderProvider _updateBuilderProvider;
        private readonly IHubContext<ClockingNotificationHub> _hubContext;
        private readonly IRealTimeDashBoardUpdate _dashboardUpdate;
        private readonly ILogger<AllActions> _logger;
        private readonly ICommon _common;


        public UniversalActions(
              ITimeSummaryRepository timeSummaries
            , IRepository<EmployeeDetails> employees
            , IRepository<CompanyEmployee> companyEmployees
            , IRepository<Company> companies
            , IRepository<User> users
            , IRepository<Loan> loans
            , IRepository<LeaveStore> leaveStore
            , IRepository<Leave> leaves
            , IRepository<History> histories
            , IRepository<Customization> customization
            , IRepository<Supplier> suppliers
            , IRepository<SupplierInvoices> suppliersInvoices
            , IRepository<ProcessedTimesSummary> processedTimesSummary
            , IQueryBuilderProvider queryBuilderProvider
            , IPasswordManager passwordManager
            , IEmployeeIdGenerator idGenerator
            , IEmployeeActions employeeActions
            , IDateTimeProvider dateTimeProvider
            , IUpdateBuilderProvider updateBuilderProvider
            , ILogger<AllActions> logger

            , IHubContext<ClockingNotificationHub> hubContext
            , IHubContext<DashboardUIHub> dashboardHubContext
,
IRealTimeDashBoardUpdate dashboardUpdate,
ICommon common)
        {
            _timeSummaries = timeSummaries;
            _employees = employees;
            _companyEmployees = companyEmployees;
            _companies = companies;
            _users = users;
            _suppliers = suppliers;
            _suppliersInvoice = suppliersInvoices;
            _processedTimesSummary = processedTimesSummary;
            _loans = loans;
            _leaveStore = leaveStore;
            _leaves = leaves;
            _histories = histories;
            _customization = customization;
            _queryBuilderProvider = queryBuilderProvider;
            _passwordManager = passwordManager;
            _idGenerator = idGenerator;
            _employeeActions = employeeActions;
            _dateTimeProvider = dateTimeProvider;
            _updateBuilderProvider = updateBuilderProvider;
            _logger = logger;
            _hubContext = hubContext;
            _dashboardUpdate = dashboardUpdate;
            _common = common;
        }

        public async Task<Response> Login(UniversalLoginModel model)
        {
            string email = model.Email.Trim().ToLowerInvariant();
            User user = _users.FindOne(x => x.Email == email).Result;
            if (user is null)
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Email😒"));


            bool matches = await _passwordManager.IsMatch(model.Password, user.Password);
            if (!matches)
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Password😒"));

            IEnumerable<CompanyEmployee> companyEmployee = await _companyEmployees.Find(x => x.EmployeeId == user.EmployeeId);
            List<ObjectId> companyIds = companyEmployee.Select(x => x.CompanyId).ToList();

            IQueryBuilder<Company> _query = _queryBuilderProvider.For<Company>().New().In(x => x.Id, companyIds);
            IEnumerable<Company> companies = await _companies.Find(_query);

            UniversalLoginResponse response = new UniversalLoginResponse();
            response.Email = email;
            response.UserCompany = companies.FirstOrDefault();

            return new Response<UniversalLoginResponse>(response);
        }

        public async Task<Response> GetPositionsInfo(string role, string? companyId = null)
        {
            Company company = await _common.ValidateCompany(role, companyId);

            List<string> positions = company.Positions;

            IEnumerable<CompanyEmployee> employees = await _companyEmployees.Find(x => x.CompanyId == company.Id && x.DeletedIndicator == false);

            List<UniversalPositionsInfo> ListOfPositionsAndItsEmployees = new();
            foreach (string position in positions)
            {
                List<UniversalCompanyEmployeeProfile> EmployeesUnderThisPosition = new();
                UniversalPositionsInfo companyPositionWithItsEmployees = new()
                {
                    Position = position
                };
                foreach (CompanyEmployee employee in employees)
                {
                    if (employee.Position == position)
                    {
                        UniversalCompanyEmployeeProfile companyEmployeeProfile = new()
                        {
                            Id = employee.Id.ToString(),
                            Name = employee.Name,
                            Surname = employee.Surname,
                            NickName = employee.NickName,
                            FullName = employee.FullName,
                            EmployeeId = employee.EmployeeId,
                            Department = employee.Department,
                            CellNumber = employee.CellNumber,
                            EmployeeAddress = employee.EmployeeAddress,
                            Roles = employee.Roles,
                            Position = employee.Position,
                            WeekDays = employee.WeekDays,
                            //TimeStamp = employee.Timestamp
                        };
                        EmployeesUnderThisPosition.Add(companyEmployeeProfile); //list of _employees per position

                    }
                }
                if (EmployeesUnderThisPosition is not null)
                {
                    companyPositionWithItsEmployees.EmployeesUnderThisPosition = new List<UniversalCompanyEmployeeProfile>();
                    companyPositionWithItsEmployees.EmployeesUnderThisPosition.AddRange(EmployeesUnderThisPosition);
                }
                if (companyPositionWithItsEmployees is not null)
                {
                    ListOfPositionsAndItsEmployees.Add(companyPositionWithItsEmployees);
                }
            }


            return new Response<IEnumerable<UniversalPositionsInfo>>(ListOfPositionsAndItsEmployees);
        }

        public async Task<Response> UniversalClockings(UniversalClockingModel model, string role, string? companyId = null)
        {

            Company company = await _common.ValidateCompany(role, companyId);

            CompanyEmployee companyEmployeeData = await _companyEmployees.FindOne(x => x.EmployeeId == model.EmployeeId && x.CompanyId == company.Id) ?? throw new HttpResponseException("No Employee Found");

            EmployeeDetails? clockingEmployee = await _employees.FindOne(x => x.EmployeeId == model.EmployeeId) ?? throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "User not found"));
            if (clockingEmployee.Pin is null)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Please set your pin first"));
            }

            bool matches = await _passwordManager.IsMatch(model.Pin, clockingEmployee.Pin);
            if (!matches)
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Pin"));

            List<EmployeeTask> PersonsDayOfTheWeek = companyEmployeeData.WeekDays.FirstOrDefault(x => x.Key.Contains(DateTime.UtcNow.ToString("ddd"))).Value;

            // again PersonsDayOfTheWeek may be null, so null check should be before getting the count and not after
            // this should be: PersonsDayOfTheWeek is null || PersonsDayOfTheWeek.Count()==0
            if (PersonsDayOfTheWeek is null || PersonsDayOfTheWeek.Count == 0)
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "You are NOT set to be on shift, Please consult with your HR or manager"));

            // get the time summary that is recording this employee's data
            TimeSummary employeeTimes = await _timeSummaries.FindOne(x => x.EmployeeId == model.EmployeeId && x.CompanyId == company.Id);


            if (model.ClockingType == ClokingType.Clockin)
            {

                ClockData clocks = ClockInDataBuilder(PersonsDayOfTheWeek, company);

                bool isHoliday = IsHoliday(clocks);
                if (isHoliday) clocks.RateType = RateType.PublicHoliday;
                TimeSummary times = new TimeSummary();
                if (employeeTimes is null) /* clocking user for the first timee*/
                {
                    times = TimeSummaryBuilder(company.Id, clockingEmployee, companyEmployeeData, clocks);

                    await _timeSummaries.Insert(times);

                }
                else /* Updating users times*/
                {
                    employeeTimes = TimesSummaryUpdator(employeeTimes, clocks);
                    employeeTimes.EndDate = DateTime.UtcNow;
                    times = await _timeSummaries.Update(employeeTimes.Id.ToString(), employeeTimes);

                    LeaveStoreFunc(companyEmployeeData.EmployeeId, company.Id);
                }

                _dashboardUpdate.OnClockingAction(companyId);
                string notification = $"{employeeTimes?.EmployeeId} has clocked in";
                await _hubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                return new() { TimeStamp = clocks.ClockIn, StatusCode = HttpStatusCode.OK, Message = "Clocked In" };
            }
            else if (model.ClockingType == ClokingType.Clockout)
            {
                if (employeeTimes is null || employeeTimes.Clocks.Count is 0 || employeeTimes.Clocks is null)
                    throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "You have NOT clocked in today. Please clock in first"));

                ClockData? lastOnList = employeeTimes?.Clocks.Where(x => x.ClockIn <= DateTime.UtcNow &&
                                                            x.IsAdminClocking == false
                                                            )
                                               .OrderByDescending(x => x.ClockIn).FirstOrDefault();
                if (lastOnList!.ClockOut is not null)
                    throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "You have NOT clocked in today. Please clock in first"));

                DateTime clockOutTime = DateTime.UtcNow;
                // update end date of time summary to reflect most recent time
                employeeTimes!.EndDate = clockOutTime;

                lastOnList!.ClockOut = clockOutTime;
                employeeTimes.EndDate = DateTime.UtcNow;
                TimeSummary? data = await _timeSummaries.Update(employeeTimes!.Id.ToString(), employeeTimes!);
                _dashboardUpdate.OnClockingAction(companyId);
                string notification = $"{employeeTimes.EmployeeId} has clocked out";
                await _hubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                return new() { TimeStamp = clockOutTime, StatusCode = HttpStatusCode.OK, Message = "Clocked Out" };
            }
            else if (model.ClockingType == ClokingType.StartBreak)
            {

                if (employeeTimes is null || employeeTimes.Clocks.Count is 0 || employeeTimes.Clocks is null)
                    throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "You have NOT clocked in today. Please clock in first"));

                ClockData? lastOnListClocksData = employeeTimes?.Clocks
                    .Where(x => x.ClockIn <= DateTime.UtcNow && x.IsAdminClocking == false)
                    .OrderByDescending(x => x.ClockIn)
                    .FirstOrDefault();

                DateTime startBreakTime = DateTime.UtcNow;
                RestTime restTime = new()
                {
                    StartTime = DateTime.UtcNow,
                    EndTime = null
                };

                lastOnListClocksData?.RestTimes.Add(restTime);
                employeeTimes!.EndDate = DateTime.UtcNow;

                TimeSummary data = await _timeSummaries.Update(employeeTimes!.Id.ToString(), employeeTimes);
                if (data is not null)
                {
                    _dashboardUpdate.OnClockingAction(companyId);
                    string notification = $"{employeeTimes.EmployeeId} has taken a break";
                    await _hubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                    return new() { TimeStamp = startBreakTime, StatusCode = HttpStatusCode.OK, Message = "Started Your Break" };
                }
                throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "something has gone wrong, Contact Stella Times Team"));
            }
            else //end break
            {
                if (employeeTimes is null || employeeTimes.Clocks.Count is 0 || employeeTimes.Clocks is null)
                    throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "You have NOT clocked in today. Please clock in first"));

                ClockData? lastOnListClocksData = employeeTimes?.Clocks
                                               .Where(x => x.ClockIn <= DateTime.UtcNow && x.IsAdminClocking == false)
                                               .OrderByDescending(x => x.ClockIn)
                                               .FirstOrDefault();

                if (lastOnListClocksData is null || lastOnListClocksData.RestTimes.Count is 0 || lastOnListClocksData.RestTimes is null)
                    throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "You have NOT taken a break, please start break first"));

                RestTime? lastOnListRestTime = lastOnListClocksData?.RestTimes
                    .Where(x => x.StartTime <= DateTime.UtcNow)
                    .OrderByDescending(x => x.StartTime)
                    .FirstOrDefault();

                DateTime endBreakTime = DateTime.UtcNow;

                lastOnListRestTime!.EndTime = endBreakTime;
                employeeTimes!.EndDate = DateTime.UtcNow;

                TimeSummary? data = await _timeSummaries.Update(employeeTimes!.Id.ToString(), employeeTimes!);
                if (data is not null)
                {
                    _dashboardUpdate.OnClockingAction(companyId);
                    string notification = $"{employeeTimes.EmployeeId} is back from break";
                    await _hubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                    return new() { TimeStamp = endBreakTime, StatusCode = HttpStatusCode.OK, Message = "Ended Your Break" };
                }
                throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "something has gone wrong, Contact Stella Times Team"));
            }
        }

        public async Task<Response> ChangeEmployeePassword(string employeeDetailsId, string password)
        {
            var employee = await _employees.FindById(employeeDetailsId);
            if (employee is null) throw new HttpResponseException("No Employee found");

            string hash = await _passwordManager.Hash(password);

            employee.Password = hash;

            await _employees.Update(employeeDetailsId, employee);
            return new Response(HttpStatusCode.OK, message: "Password changed!");
        }

        public async Task<Response> ChangeEmployeePin(ChangePin changePin)
        {
            EmployeeDetails employee = await _employees.FindOne(x => x.EmployeeId == changePin.EmployeeId);
            if (employee is null)
                throw new HttpResponseException("Employee not found");

            bool matches = await _passwordManager.IsMatch(changePin.Password, employee.Password);
            if (!matches)
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Password😒"));

            string hash = await _passwordManager.Hash(changePin.Pin);


            employee.Pin = hash;

            await _employees.Update(employee.Id.ToString(), employee);
            return new Response(HttpStatusCode.OK, message: "Your pin has been reset😊");
        }


        private static TimeSummary TimeSummaryBuilder(ObjectId companyId, EmployeeDetails userEmpID, CompanyEmployee thisEmployee, ClockData clocks)
        {
            TimeSummary times = new()
            {
                EmployeeDetailsId = userEmpID.Id,
                EmployeeId = thisEmployee.EmployeeId,
                CreatedBy = userEmpID.Id,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = userEmpID.Id,
                UpdatedDate = DateTime.UtcNow,
                DeletedIndicator = false,
                CompanyId = companyId,
                StartDate = clocks.ClockIn,
                EndDate = clocks.ClockIn

            };

            times.Clocks.Clear();
            times.Clocks.Add(clocks);

            return times;
        }
        private ClockData ClockInDataBuilder(List<EmployeeTask> personsDayOfTheWeek, Company company)
        {
            DateTime clockInTime = DateTime.UtcNow;
            ClockData clocks = new()
            {
                IsProcessed = false,
                IsAdminClocking = false,
                IsSickLeaveDays = false,
                IsFamilyLeaveDays = false,
                IsAnnualLeaveDays = false,

                ClockIn = clockInTime,

                ClockOut = null,
                RestTimes = new()
            };
            Dictionary<ObjectId, Shift> companyShiftsMapped = company.Shifts.MapUnique(x => x.Id);
            Dictionary<ObjectId, Rate> companyRatesMapped = company.Rates.MapUnique(x => x.Id);
            clocks.Shift = companyShiftsMapped[personsDayOfTheWeek.FirstOrDefault()!.Shift];
            clocks.Rate = companyRatesMapped[personsDayOfTheWeek.FirstOrDefault()!.Rate];
            clocks.RateType = personsDayOfTheWeek.FirstOrDefault()!.RateType;

            return clocks;
        }


        private static bool IsHoliday(ClockData clocks)
        {
            bool isHoliday = HolidaysHelper.IsSAHoliday();
            return isHoliday;
        }

        private TimeSummary TimesSummaryUpdator(TimeSummary employeeTimes, ClockData clocks)
        {
            DateTime clockInTime = DateTime.UtcNow;
            if (employeeTimes.Clocks.Count == 0)
                employeeTimes.StartDate = clockInTime;
            if (clockInTime < employeeTimes.StartDate)
                employeeTimes.StartDate = clockInTime;


            employeeTimes!.Clocks.Add(clocks);

            return employeeTimes;
        }

        private async void LeaveStoreFunc(string employeeId, ObjectId companyId)
        {
            var leave = _leaveStore.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == companyId).Result;
            if (leave is not null && leave.TimeStamp.Date != DateTime.UtcNow.Date)
            {
                leave.AnnualLeaveDays += decimal.Divide(15, 260);// * 0.0576923076923077;
                leave.SickLeaveDays = leave.SickLeaveDays + decimal.Divide(1, 26); // * 0.0384615384615385)
                leave.TimeStamp = DateTime.UtcNow.Date;

                _leaveStore?.Update(leave.Id.ToString(), leave);
            }

            else if (leave is null)
            {
                var user = _companyEmployees.FindOne(x => x.EmployeeId == employeeId).Result;
                leave = new LeaveStore
                {
                    AnnualLeaveDays = decimal.Divide(15, 260),// * 0.0576923076923077;
                    SickLeaveDays = decimal.Divide(1, 26), // * 0.0384615384615385)
                    FamilyLeaveDays = 3,
                    Name = user.Name,
                    Surname = user.Surname,
                    CompanyId = companyId,
                    EmployeeId = employeeId,
                    TimeStamp = DateTime.UtcNow.Date
                };
                await _leaveStore.Insert(leave);

            }


        }
    }

}
