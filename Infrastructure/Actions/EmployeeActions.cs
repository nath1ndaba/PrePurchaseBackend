using BackendServices;
using BackendServices.Actions;
using BackendServices.Actions.Admin.RealTimeServices;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Validators;
using BackendServices.Validators.ValidationData;
using Infrastructure.Helpers;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.LeaveDays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions
{
#nullable enable
    internal class EmployeeActions : IEmployeeActions
    {
        private readonly ITimeSummaryRepository _timeSummaries;
        private readonly IRepository<EmployeeDetails> _employees;
        private readonly IRepository<DetailedAd> _detailedAd;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IRepository<Company> _companies;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IRepository<Loan> _loans;
        private readonly IRepository<Leave> _leaves;
        private readonly IRepository<LeaveStore> _leaveStore;
        private readonly IRepository<History> _histories;
        private readonly IHubContext<ClockingNotificationHub> _notificationHubContext;
        private readonly IRealTimeDashBoardUpdate _dashboardUpdate;

        private readonly IPasswordManager _passwordManager;
        private readonly IValidator<ShiftValidationData, ShiftValidationResult> _shiftValidator;
        private readonly ILogger<EmployeeActions> _logger;

        //private readonly ClockService _clockService;


        public EmployeeActions(
            ITimeSummaryRepository timeSummaries
            , IRepository<EmployeeDetails> employees
            , IRepository<DetailedAd> detailedAd
            , IRepository<CompanyEmployee> companyEmployees
            , IRepository<Company> companies
            , IDateTimeProvider dateTimeProvider
            , IRepository<Loan> loans
            , IRepository<Leave> leaves
            , IRepository<LeaveStore> leaveStore
            , IRepository<History> histories
            , IHubContext<ClockingNotificationHub> clockingNotificationHubContext
            , IPasswordManager passwordManager
            , IValidator<ShiftValidationData, ShiftValidationResult> shiftValidator
            , ILogger<EmployeeActions> logger
,
IRealTimeDashBoardUpdate dashboardUpdate)
        {
            _timeSummaries = timeSummaries;
            _employees = employees;
            _detailedAd = detailedAd;
            _companyEmployees = companyEmployees;
            _companies = companies;
            _dateTimeProvider = dateTimeProvider;
            _loans = loans;
            _leaves = leaves;
            _leaveStore = leaveStore;
            _histories = histories;
            _notificationHubContext = clockingNotificationHubContext;
            _passwordManager = passwordManager;
            _shiftValidator = shiftValidator;
            _logger = logger;
            _dashboardUpdate = dashboardUpdate;
        }

        public async Task<Response> Login(EmployeeLoginModel model)
        {
            var employee = await _employees.FindOne(x => x.EmployeeId.ToLowerInvariant() == model.EmployeeId);

            if (employee is null)
                throw new HttpResponseException("Invalid User Id. Check with Admin please!");

            var isMatch = await _passwordManager.IsMatch(model.Password, employee.Password);
            if (isMatch is false)
                throw new HttpResponseException("Invalid Password!");

            if (employee.DeviceInfo is null && model.Deviceinfo is not null) /*user clocking for the 1st time with device, or using a new device*/
            {
                employee.DeviceInfo = model.Deviceinfo;
                await _employees.Update(employee.Id.ToString(), employee);
            }
            else if (model.Deviceinfo is not null && model.Deviceinfo != employee.DeviceInfo)
                throw new HttpResponseException("Have you changed phones?, Please consult with HR for assistance");

            //IEnumerable<CompanyEmployee>? companyEmployee = await _companyEmployees.Find(x => x.EmployeeId == employee.EmployeeId);
            //if (!companyEmployee.Any()) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Not registered with any company😒"));

            //foreach (CompanyEmployee cEmployee in companyEmployee)
            //{

            //}

            //if (companies.Any(x => x.LicenseExpiryDate < DateTime.UtcNow && x.Id == userResponse.CompanyId))
            //    throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            return new Response<EmployeeDetails>(employee);
        }

        public async Task<Response> ChangePassword(string employeeDetailsId, ChangePasswordModel model)
        {
            var (currentPassword, newPassword) = model;
            var employee = await _employees.FindById(employeeDetailsId);
            var isOldPasswordMatch = await _passwordManager.IsMatch(currentPassword, employee.Password);
            if (isOldPasswordMatch is false)
                throw new HttpResponseException("Current password does not match!");

            var hash = await _passwordManager.Hash(newPassword);

            employee.Password = hash;

            await _employees.Update(employeeDetailsId, employee);
            return new Response(HttpStatusCode.OK, message: "Password changed!");
        }

        public async Task<Response> FindById(string employeeDetailsId)
        {
            return new Response<EmployeeDetails>(await _employees.FindById(employeeDetailsId));
        }

        public async Task<Response> FindByEmployeeId(string employeeId)
        {
            employeeId = employeeId.ToLowerInvariant();
            var employee = await _employees.FindOne(x => x.EmployeeId.ToLowerInvariant() == employeeId);
            return new Response<EmployeeDetails>(employee);
        }

        public async Task<Response> ClockInOut(ClockInAndOutData clockInOutData, string employeeId, string employeeDetailsId)
        {
            Company? company = await _companies.FindById(clockInOutData.QRCode);
            if (company is null) throw new ArgumentNullException($"Invalid Company Id ({clockInOutData.QRCode})");
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            CompanyEmployee? employee = await _companyEmployees.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId.ToLowerInvariant());
            if (employee is null) throw new ArgumentNullException($"Invalid employee Id ({employeeId})");


            // get the time summary that is recording this employee's data
            TimeSummary timeSummary = await _timeSummaries.TimeSummaryByEmployeeDetailsAndCompanyId(employeeDetailsId, clockInOutData.QRCode) ?? new TimeSummary();

            //check if company is permiting mobile clocking
            if (!company.IsMobileClockingActive)
            {
                //_clockService.ClockingActionPerformed += HandleClockingAction;
                string notification = $"{employee.Name} attempted to clock in with her phone";
                await _notificationHubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                throw new HttpResponseException($"{company.CompanyName} does not permit Mobile Clocking System");
            }

            _logger.LogInformation($"timeSummary: {timeSummary?.Id}");

            ClockData? clockData = timeSummary?.Clocks.Where(x => x.ClockIn <= DateTime.UtcNow && x.IsAdminClocking == false)
                                                     .OrderByDescending(x => x.ClockIn).FirstOrDefault() ?? new ClockData();

            //
            // WE ARE CLOCKING THE USER IN BELOW HERE!!
            // 1. User has clocked before,
            // 2. User has not clocked out yet or completely did not clock out (forgot)
            // 3. Estimated overtime elapsed
            // 4. Shift starts late and end following day
            //
            //1.   timeSummary is not null &&
            //2.   clockData.ClockOut is null &&
            //3.   systemToleratedOverTime <= DateTime.Now
            //4.   clockData.Shift.ShiftEndTime < clockData.Shift.ShiftStartTime &&
            //5.   clockData.ClockIn.Date == DateTime.Now.AddDays(-1) &&
            ShiftValidationResult result = new();

            if (timeSummary is null)
            {
                ShiftValidationData validationData = new(clockInOutData, employeeId, employeeDetailsId, _dateTimeProvider.TimeTolerance, company.DistanceTolerance);

                result = await _shiftValidator.Validate(validationData);

                LeaveStoreFunc(employeeId, company.Id);
                string notification = $"{employee.Name} attempted to clock in with her phone";
                await _notificationHubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                return result.Response;
            }

            else if (timeSummary is not null && timeSummary?.Clocks.Where(x => x.IsAdminClocking == false && x.ClockIn <= DateTime.UtcNow).Count() != 0)
            {
                TimeSpan shifthours = TimeSpan.Zero;
                //if(clockData.IsLeaveDays ==true)

                shifthours = clockData.Shift.ShiftEndTime - clockData.Shift.ShiftStartTime;
                DateTime systemToleratedOverTime = clockData.ClockIn.AddHours(shifthours.Hours).AddHours(4); //tolerated 2 hours as overtime

                if (clockData.ClockOut is null && systemToleratedOverTime <= DateTime.UtcNow) //clock user in since he forgot to clock out
                {
                    ShiftValidationData validationData = new(clockInOutData, employeeId, employeeDetailsId, _dateTimeProvider.TimeTolerance, company.DistanceTolerance);
                    result = await _shiftValidator.Validate(validationData);
                    LeaveStoreFunc(timeSummary!.EmployeeId, company.Id);

                    //_clockService.ClockingActionPerformed += HandleClockingAction;
                    string notification = $"{employee.Name} has {result.Response.Message}";
                    await _notificationHubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                    _dashboardUpdate.OnClockingAction(company.Id.ToString());

                    return result.Response;
                }
                else if (clockData.ClockOut is not null)
                {
                    ShiftValidationData validationData = new(clockInOutData, employeeId, employeeDetailsId, _dateTimeProvider.TimeTolerance, company.DistanceTolerance);
                    result = await _shiftValidator.Validate(validationData);
                    LeaveStoreFunc(timeSummary!.EmployeeId, company.Id);
                    //_clockService.ClockingActionPerformed += HandleClockingAction;
                    string notification = $"{employee.Name} has {result.Response.Message}";
                    await _notificationHubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                    _dashboardUpdate.OnClockingAction(company.Id.ToString());
                    return result.Response;
                }
                else
                {

                    List<Location> companySites = company.Address.ListOfSitesPerCompany;
                    double tolerance = company.DistanceTolerance;
                    foreach (Location companySite in companySites)
                    {
                        if (ModelHelpers.WithInRadius(clockInOutData.EmployeePosition, companySite, tolerance))
                        {

                            DateTime clockOutTime = _dateTimeProvider.Now;
                            // update end date of time summary to reflect most recent time
                            timeSummary!.EndDate = clockOutTime;

                            clockData!.ClockOut = clockOutTime;
                            await _timeSummaries.Update(timeSummary!.Id.ToString(), timeSummary!);

                            //_clockService.ClockingActionPerformed += HandleClockingAction;
                            string notification = $"{employee.Name} has clocked out";
                            await _notificationHubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                            _dashboardUpdate.OnClockingAction(company.Id.ToString());
                            return new() { TimeStamp = clockOutTime, StatusCode = HttpStatusCode.OK, Message = "Clocked Out" };
                        }
                    }
                    throw new HttpResponseException($"Try getting closer. You need to be within {tolerance:.} meters of your work location to clock out!");
                }
            }

            else if (timeSummary is not null && timeSummary?.Clocks.Where(x => x.IsAdminClocking == false && x.ClockIn <= DateTime.UtcNow).Count() == 0) //data been processed by admin hence count is zero
            {
                ShiftValidationData validationData = new(clockInOutData, employeeId, employeeDetailsId, _dateTimeProvider.TimeTolerance, company.DistanceTolerance);

                result = await _shiftValidator.Validate(validationData);

                LeaveStoreFunc(timeSummary!.EmployeeId, company.Id);

                //_clockService.ClockingActionPerformed += HandleClockingAction;
                string notification = $"{employee.Name} has {result.Response.Message}";
                await _notificationHubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                _dashboardUpdate.OnClockingAction(company.Id.ToString());
                return result.Response;
            }

            //
            // 1. User has never clocked in before,
            // 2. User has clocked in before but all the data has recently been processed, hence no clocks
            // 3. User has clocked out, and ready for new shift
            else
            {
                ShiftValidationData validationData = new(clockInOutData, employeeId, employeeDetailsId, _dateTimeProvider.TimeTolerance, company.DistanceTolerance);

                result = await _shiftValidator.Validate(validationData);

                //_clockService.ClockingActionPerformed += HandleClockingAction;
                string notification = $"{employee.Name} has  {result.Response.Message}";
                await _notificationHubContext.Clients.Group(company.Id.ToString()).SendAsync("ReceiveClockingNotification", notification);
                _dashboardUpdate.OnClockingAction(company.Id.ToString());
                return result.Response;

            }
        }

        public async Task<Response> GetDetailedAds()
        {
            byte[]? emptyByte = null;
            IEnumerable<DetailedAd> ads = await _detailedAd.Find(x => x.MainAdImage != emptyByte);
            return new Response<IEnumerable<DetailedAd>>(ads);
        }
        public async Task<Response> GetClockedInStatus(string companyId, string employeeDetailsId)
        {
            Company? company = await _companies.FindById(companyId) ?? throw new HttpResponseException("Invalid Company Id");
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            // get the time summary that is recording this employee's data
            Task<TimeSummary> timeSummaryTask = _timeSummaries.TimeSummaryByEmployeeDetailsAndCompanyId(employeeDetailsId, company.Id.ToString());

            TimeSummary? timeSummary = await timeSummaryTask ?? new();

            _logger.LogInformation($"timeSummary: {timeSummary?.Id}");

            ClockData clockData = timeSummary?.Clocks.Where(x => x.IsAdminClocking == false && x.ClockIn <= DateTime.UtcNow).OrderByDescending(x => x.ClockIn).FirstOrDefault()! ?? new ClockData();

            if (timeSummary is null) return new Response<bool>(false);

            else if (timeSummary is not null && timeSummary?.Clocks.Where(x => x.IsAdminClocking == false && x.ClockIn <= DateTime.UtcNow).Count() != 0)
            {
                TimeSpan shifthours = TimeSpan.Zero;
                shifthours = clockData.Shift.ShiftEndTime - clockData.Shift.ShiftStartTime;
                var systemToleratedOverTime = clockData.ClockIn.AddHours(shifthours.Hours).AddHours(2); //tolerated 2 hours as overtime

                if (clockData.ClockOut is null && systemToleratedOverTime <= DateTime.UtcNow) //clock user in since he forgot to clock out
                    return new Response<bool>(false);
                else if (clockData.ClockOut is not null)
                    return new Response<bool>(false);
                else
                    return new Response<bool>(true);

            }

            else
                return new Response<bool>(false);



            //if (timeSummary is null || timeSummary!.Clocks.Count() == 0 || clockData.ClockOut is not null)
            //    return new Response<bool>(false);
            //else
            //    return new Response<bool>(true);
        }

        public async Task<Response> Histories(Expression<Func<History, bool>> filter, int skip = 0, int limit = 100)
        {
            IEnumerable<History> histories = await _histories.Find(filter, skip, limit);
            histories = histories.OrderByDescending(x => x.Timestamp).ToList();
            return new Response<IEnumerable<History>>(histories);
        }

        public async Task<Response> HistoriesByEmployeeDetailsId(string employeeDetailsId, string companyId, int skip = 0, int limit = 100)
        {
            if (companyId is null)
            {
                EmployeeDetails? employeeInfo = await _employees.FindById(employeeDetailsId);
                if (employeeInfo is null)
                {
                    throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "employee not found"));
                }
                CompanyEmployee companyEmployee = await _companyEmployees.FindOne(x => x.EmployeeId == employeeInfo.EmployeeId);
                if (companyEmployee is null)
                {
                    throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "employee info found"));
                }
                companyId = companyEmployee.CompanyId.ToString();
            }

            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            return await Histories(x => x.EmployeeDetailsId == ObjectId.Parse(employeeDetailsId) && x.CompanyId == ObjectId.Parse(companyId), skip, limit);
        }

        public async Task<Response> History(Expression<Func<History, bool>> filter)
        {
            return new Response<History>(await _histories.FindOne(filter));
        }

        public async Task<Response> HistoryById(string employeeDetailsId, string id)
        {
            var result = await _histories.FindById(id);
            if (result is null || result.EmployeeDetailsId == ObjectId.Parse(employeeDetailsId))
                return new Response<History>(result);

            throw new HttpResponseException(new Response(HttpStatusCode.Forbidden, error: "You are not allowed to access this resource!"));
        }

        public async Task<Response> RequestLoan(string createdBy, string updatedBy, RequestLoanModel model, string employeeId, string companyId)
        {
            if (ObjectId.TryParse(companyId, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");

            // check that this employee works for the company they are requesting a loan from
            employeeId = employeeId.ToLowerInvariant();
            var companyEmployeeTask = _companyEmployees.FindOne(x => x.EmployeeId.ToLowerInvariant() == employeeId && x.CompanyId == _companyId);
            var employeeDetailsTask = FindByEmployeeId(employeeId);

            await Task.WhenAll(companyEmployeeTask, employeeDetailsTask);

            CompanyEmployee companyEmployee = await companyEmployeeTask ?? throw new HttpResponseException("You do not work for this company!!");

            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            Response<EmployeeDetails> employeeDetails = (Response<EmployeeDetails>)employeeDetailsTask.Result;

            DateTime lastPaymentDate = DateTime.UtcNow.AddMonths(decimal.ToInt32(model.LoanDurationInMonths));
            Loan loan = new()
            {
                CreatedBy = ObjectId.Parse(createdBy),
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = ObjectId.Parse(updatedBy),
                UpdatedDate = DateTime.UtcNow,
                DeletedIndicator = false,
                LoanAmount = model.LoanAmount,
                LoanDurationInMonths = model.LoanDurationInMonths,
                LastPaymentDate = lastPaymentDate,
                Reason = model.Reason,
                CompanyId = _companyId,
                EmployeeSummary = (EmployeeSummary)employeeDetails.Data!,
                TimeStamp = _dateTimeProvider.Now,
                Department = companyEmployee.Department,
                Position = companyEmployee.Position
            };

            await _loans.Insert(loan);

            return new Response(HttpStatusCode.Created, message: "Loan requested");
        }

        public async Task<Response> RemoveLoan(string companyId, string loanId, string employeeId)
        {
            if (ObjectId.TryParse(loanId, out var _loanId) is false)
                throw new HttpResponseException("Invalid loanId!!");

            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            employeeId = employeeId.ToLowerInvariant();
            var deletedCount = await _loans.DeleteOne(x => x.Id == _loanId && x.EmployeeSummary.EmployeeId == employeeId && x.CompanyId == ObjectId.Parse(companyId));

            if (deletedCount == 0)
                throw new HttpResponseException("No documents where deleted!");

            return new Response(HttpStatusCode.OK, message: $"{deletedCount} documents where deleted!");
        }

        public async Task<Response> Loan(Expression<Func<Loan, bool>> filter)
        {
            return new Response<Loan>(await _loans.FindOne(filter));
        }

        public async Task<Response> LoansByEmployeeDetailsId(string companyId, string id, int skip = 0, int limit = 100)
        {
            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            var result = await _loans.Find(x => x.EmployeeSummary.Id == ObjectId.Parse(id) && x.CompanyId == ObjectId.Parse(companyId), skip, limit);
            return new Response<IEnumerable<Loan>>(result);
        }

        public async Task<Response> LoanById(string employeeDetailsId, string id)
        {

            var result = await _loans.FindById(id);

            if (result is null || result.EmployeeSummary.Id == ObjectId.Parse(employeeDetailsId))
                return new Response<Loan>(result);

            throw new HttpResponseException(new Response(HttpStatusCode.Forbidden, error: "You are not allowed to access this resource!"));
        }

        public async Task<Response> Loans(Expression<Func<Loan, bool>> filter, int skip = 0, int limit = 100)
        {
            var result = await _loans.Find(filter, skip, limit);
            return new Response<IEnumerable<Loan>>(result);
        }

        public async Task<Response> RequestLeave(string createdBy, string updatedBy, LeaveStatus status, RequestLeaveModel model, string employeeId, string companyId)
        {
            if (ObjectId.TryParse(companyId, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");

            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            // check that this employee works for the company they are requesting a loan from
            employeeId = employeeId.ToLowerInvariant();
            Task<CompanyEmployee> companyEmployeeTask = _companyEmployees.FindOne(x => x.EmployeeId.ToLowerInvariant() == employeeId && x.CompanyId == _companyId);
            Task<Response> employeeDetailsTask = FindByEmployeeId(employeeId);

            await Task.WhenAll(companyEmployeeTask, employeeDetailsTask);

            CompanyEmployee? companyEmployee = await companyEmployeeTask;

            if (companyEmployee is null)
                throw new HttpResponseException("You do not work for this company!!");

            Response<EmployeeDetails> employeeDetails = (Response<EmployeeDetails>)employeeDetailsTask.Result;

            Leave leave = new()
            {
                CreatedBy = ObjectId.Parse(createdBy),
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = ObjectId.Parse(updatedBy),
                UpdatedDate = DateTime.UtcNow,
                DeletedIndicator = false,
                LeaveStartDate = model.LeaveStartDate,
                LeaveEndDate = model.LeaveEndDate,
                TypeOfLeave = model.TypeOfLeave,
                DaysToTake = model.DaysToTake,
                CompanyId = _companyId,
                EmployeeSummary = (EmployeeSummary)employeeDetails.Data!,
                TimeStamp = _dateTimeProvider.Now,
                Department = companyEmployee.Department,
                Position = companyEmployee.Position,
                Status = status

            };
            leave.UpdatedDate = DateTime.UtcNow;

            await _leaves.Insert(leave);

            return new Response(HttpStatusCode.Created, message: "Leave requested.");
        }

        public async Task<Response> RemoveLeave(string updatedBy, string companyId, string leaveId, string employeeId)
        {
            if (ObjectId.TryParse(leaveId, out var _leaveId) is false)
                throw new HttpResponseException("Invalid leaveId!!");

            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            employeeId = employeeId.ToLowerInvariant();
            Leave softDelete = await _leaves.FindOne(x => x.Id == _leaveId && x.EmployeeSummary.EmployeeId == employeeId && x.CompanyId == ObjectId.Parse(companyId));
            if (softDelete != null)
            {
                softDelete.UpdatedDate = DateTime.UtcNow;
                softDelete.UpdatedBy = ObjectId.Parse(updatedBy);
                softDelete.DeletedIndicator = true;
                Leave updateleave = await _leaves.Update(leaveId, softDelete);
            }
            else throw new HttpResponseException("No documents where deleted!");

            return new Response(HttpStatusCode.OK, message: $"Leave for {softDelete.EmployeeSummary.Name} has been deleted!");
        }

        public async Task<Response> Leave(Expression<Func<Leave, bool>> filter)
        {
            return new Response<Leave>(await _leaves.FindOne(filter));
        }

        public async Task<Response> LeavesByEmployeeDetailsId(string companyId, string employeeDetailsId, int skip = 0, int limit = 100)
        {
            if (ObjectId.TryParse(employeeDetailsId, out var oId) is false)
                throw new HttpResponseException("Invalid employeeDetailsId!!");

            var result = await _leaves.Find(x => x.EmployeeSummary.Id == oId && x.CompanyId == ObjectId.Parse(companyId), skip, limit);
            return new Response<IEnumerable<Leave>>(result);
        }

        public async Task<Response> LeaveById(string employeeDetailsId, string id)
        {
            if (ObjectId.TryParse(id, out var _) is false || ObjectId.TryParse(employeeDetailsId, out var oEId) is false)
                throw new HttpResponseException("Invalid id/ids!!");

            var result = await _leaves.FindById(id);
            if (result is null || result.EmployeeSummary.Id == oEId)
                return new Response<Leave>(result);

            throw new HttpResponseException(new Response(HttpStatusCode.Forbidden, "You are not allowed to access this resource!"));
        }
        public async Task<Response> LeaveStoreByEmployeeId(string companyId, string employeeId)
        {
            var result = await _leaveStore.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == ObjectId.Parse(companyId));

            return new Response<LeaveStore>(result);
        }
        public async Task<Response> Leaves(Expression<Func<Leave, bool>> filter, int skip = 0, int limit = 100)
        {
            var result = await _leaves.Find(filter, skip, limit);
            return new Response<IEnumerable<Leave>>(result);
        }

        public async Task<Response> Rosta(string employeeId, string companyId)
        {
            if (ObjectId.TryParse(companyId, out var oId) is false)
                throw new HttpResponseException("Invalid companyId!!");

            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            employeeId = employeeId.ToLowerInvariant();
            CompanyEmployee result = await _companyEmployees.FindOne(x => x.EmployeeId.ToLowerInvariant() == employeeId && x.CompanyId == oId);

            Dictionary<ObjectId, Shift> companyShiftsMapped = company.Shifts.MapUnique(x => x.Id);
            Dictionary<ObjectId, Rate> companyRatesMapped = company.Rates.MapUnique(x => x.Id);
            Dictionary<ObjectId, Location> companySitesMapped = company.Address.ListOfSitesPerCompany.MapUnique(x => x.Id);

            Dictionary<string, IEnumerable<DetailedEmployeeTask>> _detailedRosta = new();

            if (result is { WeekDays.Count: > 0 })
            {
                _detailedRosta = DetailedCompanyEmployee.CreateDetailedTasks(result.WeekDays, companyShiftsMapped, companyRatesMapped, companySitesMapped);
            }

            return new Response<IDictionary<string, IEnumerable<DetailedEmployeeTask>>>(_detailedRosta);
        }

        public async Task<Response> CompanyProfiles(string employeeId)
        {
            IQueryable<CompanyEmployee> employeeQueryable = _companyEmployees.AsQueryable();
            IQueryable<Company> companyQueryable = _companies.AsQueryable();

            employeeId = employeeId.ToLowerInvariant();
            IQueryable<CompanyEmployee> employeeFiltered = employeeQueryable.Where(x => x.EmployeeId.ToLowerInvariant() == employeeId);

            var query = from employee in employeeFiltered
                        join company in companyQueryable on employee.CompanyId equals company.Id into joinedCompany
                        select new { employee, joinedCompany };
            IEnumerable<CompanyEmployeeProfile> result = query.AsEnumerable().Select(x => ModelHelpers.From(x.employee, x.joinedCompany));

            return await ValueTask.FromResult(new Response<IEnumerable<CompanyEmployeeProfile>>(result));
        }

        public async Task<Response> TimeSummaries(Expression<Func<TimeSummary, bool>> filter, int skip = 0, int limit = 100)
        {
            var result = await _timeSummaries.Find(filter, skip, limit);
            return new Response<IEnumerable<TimeSummary>>(result);
        }

        public async Task<Response> TimeSummariesForRange(string companyId, DateTime start, DateTime end, int skip = 0, int limit = 100)
        {
            if (ObjectId.TryParse(companyId, out var _) is false)
                throw new HttpResponseException("Invalid companyId!!");

            Company company = await _companies.FindById(companyId);
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            IEnumerable<TimeSummaryWithEmployeeDetails>? result = await _timeSummaries.FindTimeSummariesForRange(companyId, start, end, skip, limit);
            return new Response<IEnumerable<TimeSummaryWithEmployeeDetails>>(result);
        }

        public async Task<Response> TimeSummary(Expression<Func<TimeSummary, bool>> filter)
        {
            return new Response<TimeSummary>(await _timeSummaries.FindOne(filter));
        }

        private readonly List<ClockData> RestrructuredClockDataWithClockout = new();
        public async Task<Response> TimeSummariesByEmployeeDetailsId(string id, int skip = 0, int limit = 100)
        {
            if (ObjectId.TryParse(id, out var oId) is false)
                throw new HttpResponseException("Invalid employeeDetailsId!!");

            IEnumerable<TimeSummary> result = await _timeSummaries.Find(x => x.EmployeeDetailsId == ObjectId.Parse(id));
            // IEnumerable<TimeSummary> result  result = await _timeSummaries.Find(x => x.EmployeeDetailsId == ObjectId.Parse(id) && x.CompanyId==ObjectId.Parse(companyId));
            if (result == null)
                throw new HttpResponseException("No data for this user");

            IEnumerable<TimeSummary>? reStructuredTimesSummaryWithClockOut;
            if (result.FirstOrDefault(x => x.EmployeeDetailsId == oId) is not null)
            {
                List<ClockData> originialClocks = new();
                originialClocks = result.FirstOrDefault(x => x.EmployeeDetailsId == oId)!.Clocks.Where(x => x.ClockIn <= DateTime.UtcNow).ToList() ?? new List<ClockData>();
                if (originialClocks.Count != 0)
                {
                    originialClocks.OrderByDescending(x => x.ClockIn);
                    RestrructuredClockDataWithClockout.Clear();
                    OrgClocks(originialClocks, 0);
                }
                reStructuredTimesSummaryWithClockOut = result.ToList();
                reStructuredTimesSummaryWithClockOut.FirstOrDefault()!.Clocks = RestrructuredClockDataWithClockout ?? new List<ClockData>();

                TimeSummary? timeSummary = reStructuredTimesSummaryWithClockOut.FirstOrDefault();
                if (timeSummary != null)
                {
                    Company company = await _companies.FindOne(x => x.Id == timeSummary.CompanyId);
                    if (company is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "company is not known😒"));

                    if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));
                }

                return new Response<IEnumerable<TimeSummary>>(reStructuredTimesSummaryWithClockOut);
            }
            return new Response<IEnumerable<TimeSummary>>(null);
        }
        public async Task<Response> MobileWalletByEmployeeDetailsId(string id, string companyId)
        {
            if (ObjectId.TryParse(id, out var oId) is false)
                throw new HttpResponseException("Invalid employeeDetailsId!!");

            Company company = await _companies.FindById(companyId);
            if (company is null) throw new HttpResponseException("Invalid Company Id");
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            var result = await _timeSummaries.Find(x => x.EmployeeDetailsId == ObjectId.Parse(id));
            if (result == null)
                throw new HttpResponseException("No data for this user");

            IEnumerable<TimeSummary>? reStructuredTimesSummaryWithClockOut;
            MobileWallet wallet = new();

            if (result.FirstOrDefault(x => x.EmployeeDetailsId == oId) is not null)
            {
                List<ClockData> originialClocks = new();
                originialClocks = result.FirstOrDefault(x => x.EmployeeDetailsId == oId)!.Clocks.Where(x => x.ClockIn <= DateTime.UtcNow).ToList() ?? new List<ClockData>();
                if (originialClocks.Count != 0)
                {
                    originialClocks.OrderByDescending(x => x.ClockIn);
                    RestrructuredClockDataWithClockout.Clear();
                    OrgClocks(originialClocks, 0);

                    reStructuredTimesSummaryWithClockOut = result.ToList();
                    reStructuredTimesSummaryWithClockOut.FirstOrDefault()!.Clocks = RestrructuredClockDataWithClockout ?? new List<ClockData>();


                    //get amount accumilated over the month
                    var amounts = reStructuredTimesSummaryWithClockOut.GetAmounts();
                    List<RateType> amountTypes = new List<RateType>() { RateType.Standard, RateType.Saturday, RateType.PublicHoliday, RateType.Sunday };
                    IEnumerable<RateContent> _amounts = amountTypes.Select(x =>
                    {
                        var @groups = amounts.Where(m => m.ratetype == x);
                        if (@groups.Any())
                        {
                            var @group = @groups.First();
                            var amount = @group.clocks.Sum(x => x.Amount - x.Rate.OverTimeRate * (decimal)x.OverTimeHours);


                            return new RateContent
                            {
                                RateType = @group.ratetype.ToString(),
                                RateAmount = amount.ToString("0.00")
                            };
                        }

                        return new RateContent
                        {

                            RateType = x.ToString(),
                            RateAmount = "0.00"
                        };
                    });

                    var clocks = reStructuredTimesSummaryWithClockOut.SelectMany(x => x.Clocks);


                    var mOverTime = clocks.Where(x => x.OverTimeHours > 0)
                                    .Select(x => x.Rate.OverTimeRate * (decimal)x.OverTimeHours)
                                    .Sum();
                    _amounts = _amounts.Append(new RateContent { RateType = "Overtime ", RateAmount = mOverTime.ToString("0.00") });

                    var grossAmount = clocks.Sum(x => x.Amount);
                    var dailyRate = new decimal();
                    dailyRate = company.Rates.Where(x => x.NameOfPosition == reStructuredTimesSummaryWithClockOut.FirstOrDefault()!.Clocks.FirstOrDefault()!.Rate.NameOfPosition).FirstOrDefault()!.DailyBonus;

                    var bonus = reStructuredTimesSummaryWithClockOut.FirstOrDefault()!.Clocks.GroupBy(x => (x.ClockIn.Year, x.ClockIn.DayOfYear)).Count() * dailyRate;
                    var grossNbonus = clocks.Sum(x => x.Amount) + bonus;
                    var uifAmount = grossNbonus * (decimal)0.01;// Minus UIF
                    var taxAmount = clocks.Sum(x => x.Amount * (decimal)0.0);
                    var netAmount = grossAmount - uifAmount - taxAmount;

                    wallet.BonusAmount = bonus;
                    wallet.RateContents = _amounts;
                    wallet.GrossAmount = grossAmount;
                    wallet.UifAmount = uifAmount;
                    wallet.TaxAmount = taxAmount;
                    wallet.NetAmount = netAmount;

                }
            }

            return new Response<MobileWallet>(wallet);

        }

        public async Task<Response> TimeSummaryById(string employeeDetailsId, string id)
        {
            if (ObjectId.TryParse(id, out var _) is false || ObjectId.TryParse(employeeDetailsId, out var oEId) is false)
                throw new HttpResponseException("Invalid id/ids!!");

            var result = await _timeSummaries.FindById(id);

            if (result is null || result.EmployeeDetailsId == oEId)
                return new Response<TimeSummary>(result);

            throw new HttpResponseException(new Response(HttpStatusCode.Forbidden, error: "You are not allowed to access this resource!"));
        }
        //}

        public async Task<Response> GetLeaveStore(string id, int skip = 0, int limit = 100)
        {
            var result = await _leaves.Find(x => x.EmployeeSummary.Id == ObjectId.Parse(id), skip, limit);
            return new Response<IEnumerable<Leave>>(result);
        }

        private void OrgClocks(List<ClockData> clock, int len)
        {
            if (clock.Count == len)
                return;


            TimeSpan shifthours = clock[len].Shift.ShiftEndTime - clock[len].Shift.ShiftStartTime;
            var systemToleratedOverTime = clock[len].ClockIn.AddHours(shifthours.Hours).AddHours(2); //tolerated 2 hours as overtime


            if (clock[len].ClockOut is null && systemToleratedOverTime >= DateTime.UtcNow) //3)
                clock[len].ClockOut = DateTime.UtcNow;

            else if (clock[len].ClockOut == null && systemToleratedOverTime < DateTime.UtcNow)
            {
                var referenceDate = clock[len].ClockIn!.Date.ToShortDateString() + " " + clock[len].Shift.ShiftEndTime.ToString();


                clock[len].ClockOut = DateTime.Parse(referenceDate);
            }
            RestrructuredClockDataWithClockout.Add(clock[len]);
            OrgClocks(clock, len + 1);
            return;
        }

        private async void LeaveStoreFunc(string employeeId, ObjectId companyId)
        {
            Company company = await _companies.FindById(companyId.ToString()) ?? throw new HttpResponseException("Invalid Company Id");
            if (company.LicenseExpiryDate < DateTime.UtcNow) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));

            var leave = _leaveStore.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == companyId).Result;
            var user = _companyEmployees.FindOne(x => x.EmployeeId == employeeId).Result;

            if (leave is not null && leave.TimeStamp.Date.Date != DateTime.UtcNow.Date)
            {
                leave.AnnualLeaveDays += decimal.Divide(15, 260);// * 0.0576923076923077;
                leave.SickLeaveDays += decimal.Divide(1, 26); // * 0.0384615384615385)
                leave.Name = user.Name;
                leave.Surname = user.Surname;
                leave.TimeStamp = _dateTimeProvider.Now.Date;
                _leaveStore?.Update(leave.Id.ToString(), leave);
            }

            else if (leave is null)
            {
                leave = new LeaveStore();
                leave.AnnualLeaveDays = decimal.Divide(15, 260);// * 0.0576923076923077;
                leave.SickLeaveDays = decimal.Divide(1, 26); // * 0.0384615384615385)
                leave.FamilyLeaveDays = 3;
                leave.Name = user.Name;
                leave.Surname = user.Surname;
                leave.CompanyId = companyId;
                leave.EmployeeId = employeeId;
                leave.TimeStamp = _dateTimeProvider.Now.Date;
                await _leaveStore.Insert(leave);

            }


        }

        private void HandleClockingAction(object sender, string actionMessage)
        {
            // Handle the action message received from the ClockService
            Console.WriteLine("Received action message: " + actionMessage);
        }
#nullable disable
    }
}
