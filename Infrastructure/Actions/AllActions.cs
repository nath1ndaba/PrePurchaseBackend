using BackendServices;
using BackendServices.Actions;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Infrastructure.Helpers;
using MongoDB.Bson;
using PrePurchase.Models;
using PrePurchase.Models.StatementsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using PrePurchase.Models.Requests;
using System.Text.Json;
using PrePurchase.Models.LeaveDays;
using PrePurchase.Models.Payments;
using System.Data;
using System.Diagnostics;
using PrePurchase.Models.HistoryModels;

namespace Infrastructure.Actions
{
#nullable enable
    internal class AllActions : IAllActions
    {
        //this 
        private readonly ITimeSummaryRepository timeSummaries;
        private readonly IRepository<DetailedAd> detailedAd;
        private readonly IRepository<EmployeeDetails> employees;
        private readonly IRepository<CompanyEmployee> companyEmployees;
        private readonly IRepository<Company> _companies;
        private readonly IRepository<DiscontinuedUser> users;
        private readonly IRepository<Loan> loans;
        private readonly IRepository<LeaveStore> leaveStore;
        private readonly IRepository<Leave> leaves;
        private readonly IRepository<History> histories;
        private readonly IRepository<Customization> customization;
        private readonly IRepository<Supplier> suppliers;
        private readonly IRepository<SupplierInvoices> suppliersInvoice;
        private readonly IRepository<ProcessedTimesSummary> processedTimesSummary;
        private readonly IQueryBuilderProvider queryBuilderProvider;
        private readonly IPasswordManager passwordManager;
        private readonly IEmployeeIdGenerator idGenerator;
        private readonly IEmployeeActions employeeActions;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IUpdateBuilderProvider updateBuilderProvider;
        private readonly ILogger<AllActions> logger;
        private readonly ICommon _common;



        public AllActions(
              ITimeSummaryRepository timeSummaries
            , IRepository<DetailedAd> detailedAd
            , IRepository<EmployeeDetails> employees
            , IRepository<CompanyEmployee> companyEmployees
            , IRepository<Company> companies
            , IRepository<DiscontinuedUser> users
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
            , ILogger<AllActions> logger,
ICommon common)
        {
            this.timeSummaries = timeSummaries;
            this.detailedAd = detailedAd;
            this.employees = employees;
            this.companyEmployees = companyEmployees;
            this._companies = companies;
            this.users = users;
            this.suppliers = suppliers;
            this.suppliersInvoice = suppliersInvoices;
            this.processedTimesSummary = processedTimesSummary;
            /////this.timeTables = timeTables;
            this.loans = loans;
            this.leaveStore = leaveStore;
            this.leaves = leaves;
            this.histories = histories;
            this.customization = customization;
            this.queryBuilderProvider = queryBuilderProvider;
            this.passwordManager = passwordManager;
            this.idGenerator = idGenerator;
            this.employeeActions = employeeActions;
            this.dateTimeProvider = dateTimeProvider;
            this.updateBuilderProvider = updateBuilderProvider;
            this.logger = logger;
            _common = common;
        }

        public async Task<Response> UploadAds(DetailedAd model, string createdBy, string updatedBy, string role, string? companyId = null)
        {
            Company company = await _common.ValidateCompany(role, companyId);

            model.Id = ObjectId.GenerateNewId();
            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;
            await detailedAd.Insert(model);

            return new Response<DetailedAd>(model);
        }

        public async Task<Response> GetDeatiledAds(string role, string? companyId = null)
        {
            Company company = await _common.ValidateCompany(role, companyId);

            IEnumerable<DetailedAd> ads = new List<DetailedAd>();
            if (company.IsHiddenEnv is true)
            {

                byte[]? adsByte = null;
                ads = await detailedAd.Find(x => x.MainAdImage != adsByte);
            }
            return new Response<IEnumerable<DetailedAd>>(ads);

        }

        public async Task<Response> GetprocessedTimesSummaries(string role, string? companyId = null)
        {
            Company company = await _common.ValidateCompany(role, companyId);

            IEnumerable<ProcessedTimesSummary> times = await processedTimesSummary.Find(x => x.CompanyId == company.Id);

            return new Response<IEnumerable<ProcessedTimesSummary>>(times);

        }

        public async Task<Response> GetProcessedTimesSummariesBatch(string role, string batchCode, string? companyId = null)
        {
            Company company = await _common.ValidateCompany(role, companyId);

            IEnumerable<ProcessedTimesSummary>? times = await processedTimesSummary.Find(x => x.CompanyId == company.Id && x.HistoryBatchCode == batchCode);

            ProcessedTimesSummary eachBatch = times.FirstOrDefault() ?? new ProcessedTimesSummary();
            return new Response<ProcessedTimesSummary>(eachBatch);

        }

        public async Task<Response> UpdateEmployeeTimes(AdminManualClockings model, string role, string? companyId = null)
        {
            Company company = await _common.ValidateCompany(role, companyId);


            CompanyEmployee thisEmployee = await companyEmployees.FindOne(x => x.EmployeeId == model.StaffMemberId && x.CompanyId == company.Id);
            if (thisEmployee is null)
                throw new HttpResponseException("No Employee Found");
            Task<EmployeeDetails> userEmpID = employees.FindOne(x => x.EmployeeId == model.StaffMemberId);

            // not sure what the purpose for this is, when you already have a DateTime instance in the form of "model.ClockingDate".
            // be mindful when sending DateTime over the wire, especially when you need to consider the senders local date.
            //DateTime dateValue = new DateTime(model.ClockingDate.Ticks);
            model.ClockingDate = model.ClockingDate.OrderBy(c => c.ClockingDate).ToList();
            foreach (var date in model.ClockingDate)
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
                //ClockData TimesForSameDay = new();
                //if (employeeTimes is not null)
                //{
                //    TimesForSameDay = (ClockData)employeeTimes!.Clocks
                //        .Where(x => x.ClockIn.Date == model.StartTime.Date)
                //        .OrderBy(x => x.ClockIn);

                //    if (TimesForSameDay is not null &&
                //        employeeTimes!.Clocks.LastOrDefault()!.ClockIn >= model.EndTime &&
                //        employeeTimes.Clocks.LastOrDefault()!.ClockIn  <= model.StartTime)
                //    { 

                //    }

                //}
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
                var employeeTimes = await timeSummaries.FindOne(x => x.EmployeeId == model.StaffMemberId && x.CompanyId == company.Id);
                if (employeeTimes is null) /* clocking user for the first timee*/
                {
                    TimeSummary times = new()
                    {
                        EmployeeDetailsId = userEmpID.Result.Id,
                        EmployeeId = thisEmployee.EmployeeId,
                        CompanyId = company.Id,
                        StartDate = clocks.ClockIn,
                        EndDate = clocks.ClockIn
                    };

                    times.Clocks.Add(clocks);

                    await timeSummaries.Insert(times);

                }
                else /* Updating users times*/
                {
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

                    TimeSummary result = await timeSummaries.Update(employeeTimes.Id.ToString(), employeeTimes);

                    LeaveStoreFunc(thisEmployee.EmployeeId, company.Id);

                }
            }
            return new Response<HttpStatusCode>(HttpStatusCode.Created);



        }


        public async Task<Response> AddSupplier(string role, Supplier model, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            Company company = companyResponse.Data!;
            model.CompanyId = company.Id;

            await suppliers.Insert(model);

            return new Response<Supplier>(model, HttpStatusCode.Created);
        }

        public async Task<Response> GetSuppliers(string role, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            Company company = companyResponse.Data!;

            IEnumerable<Supplier> supplier = await suppliers.Find(x => x.CompanyId == company.Id);
            return new Response<IEnumerable<Supplier>>(supplier);
        }

        public async Task<Response> GetSuppliersByPaymentMethod(string paymentMethod, string role, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            Company company = companyResponse.Data!;
            paymentMethod = paymentMethod.ToLowerInvariant();
            IEnumerable<Supplier> supplier = await suppliers.Find(x => x.CompanyId == company.Id && x.PaymentMethod.ToLowerInvariant() == paymentMethod);

            return new Response<IEnumerable<Supplier>>(supplier);
        }

        public async Task<Response> AddSupplierInvoices(string role, SupplierInvoices model, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            await suppliersInvoice.Insert(model);
            Task<IEnumerable<SupplierInvoices>> CurrentSupplier = suppliersInvoice.Find(x => x.SupplierId == model.SupplierId);
            double TotalAmount = CurrentSupplier.Result.Sum(x => x.TotalAmount);
            Supplier CrrentSupplier = await suppliers.FindOne(x => x.Id == model.SupplierId);

            if (CrrentSupplier != null)
            {
                CrrentSupplier.TotalAmount = TotalAmount;
            }
            await suppliers.Update(model.SupplierId.ToString(), CrrentSupplier!);

            return new Response<SupplierInvoices>(model, HttpStatusCode.Created);
        }

        public async Task<Response> GetSuppliersInvoices(string role, string? companyId = null, string? supplierId = null)
        {
            Response response = await GetSupplier(role, supplierId);
            if (response is not Response<Supplier> supplierResponse)
                throw new HttpResponseException(response);

            Supplier supplier = supplierResponse.Data!;

            IEnumerable<SupplierInvoices> invoices = await suppliersInvoice.Find(x => x.SupplierId == supplier.Id);

            return new Response<IEnumerable<SupplierInvoices>>(invoices);
        }

        public async Task<Response> AddPaymentMethod(string paymentMethod, string role, string? companyId = null)
        {

            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            var company = companyResponse.Data!;

            paymentMethod = paymentMethod.Trim();
            if (company.SuppliersPaymentMethods.Contains(paymentMethod))
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: "Payment method already exists!"));
            company.SuppliersPaymentMethods.Add(paymentMethod);
            await _companies.Update(company.Id.ToString(), company);

            return new Response<string>(paymentMethod);
        }



        private List<LeaveGet> leavess = new();
        public async Task<Response> GetLeaveStore(string role, string? companyId = null)
        {
            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            var company = companyResponse.Data!;

            //TO-Do: add paramaters so you can page or skip and limit
            var _leaveStore = await leaveStore.Find(x => x.CompanyId == company.Id, limit: 100);

            //var employeesIds = _leaveStore.Select(x => x.EmployeeId);
            //var queryBuilder = queryBuilderProvider.For<CompanyEmployee>();

            //var query = queryBuilder.Eq(x => x.CompanyId, ObjectId.Parse(companyId))
            //    .And<CompanyEmployee>(queryBuilder.In(x => x.EmployeeId, employeesIds));

            //var _employees = await companyEmployees.Find(query, limit: employeesIds.Count());


            ////GettingLeaveStore(_leaveStore,0);
            //foreach (var ll in _leaveStore)
            //{
            //    LeaveGet leave = new();
            //    leave.Id = ll.Id;
            //    leave.SickLeaveDays = ll.SickLeaveDays;
            //    leave.AnnualLeaveDays= ll.AnnualLeaveDays;
            //    leave.FamilyLeaveDays = ll.FamilyLeaveDays;
            //    var employee = _employees.FirstOrDefault(y => y.EmployeeId == ll.EmployeeId);

            //    leave.CompanyId = ll.CompanyId;

            //    leave.Surname = employee?.Surname;
            //    leave.Name= employee?.Name;

            //    leavess.Add(leave);
            //}
            //leave = _leaveStore.Select(x =>
            //{
            //    var employee = _employees.FirstOrDefault(y => y.EmployeeId == x.EmployeeId);



            //    leave.Name = employee?.Name;
            //    leave.Surname = employee?.Surname;

            //    return x;
            //});

            return new Response<IEnumerable<LeaveStore>>(_leaveStore);
        }

        public async Task<Response> UpdateLeaveStore(List<UpdateLeaveStore> leave, string role, string? companyId = null)
        {
            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            var company = companyResponse.Data!;

            foreach (var eachLeave in leave)
            {
                var store = await leaveStore.FindById(eachLeave.StoreId);

                // to remove this 
                var user = companyEmployees.FindOne(x => x.EmployeeId == store.EmployeeId).Result;
                store.Name = user.Name;
                store.Surname = user.Surname;
                //to remove this

                store.FamilyLeaveDays = eachLeave.FamilyLeaveDays;
                store.SickLeaveDays = eachLeave.SickLeaveDays;
                store.AnnualLeaveDays = eachLeave.AnnualLeaveDays;

                await leaveStore.Update(store.Id.ToString(), store);
            }

            return new Response<HttpStatusCode>(HttpStatusCode.Created);
        }


        public async Task<Response> Customization(string role, Customization model, string? companyId = null)
        {
            var currentCompanyId = new ObjectId(companyId);
            model.CompanyId = currentCompanyId;

            var custData = await customization.Find(x => x.CompanyId == currentCompanyId);
            ObjectId? customID = null;

            foreach (var x in custData)
            {
                customID = x.Id;
            }
            if (customID is not null)
            {
                model.Id = customID.Value;
                await customization.Update(customID.ToString(), model);
            }
            else
                await customization.Insert(model);

            return new Response<Customization>(model, HttpStatusCode.Created);
        }

        public async Task<Response> UpdateRate(string id, RateModel model, string role, string? companyId = null)
        {
            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            var company = companyResponse.Data!;

            var toEdit = company.Rates.Find(x => x.Id == ObjectId.Parse(id));
            var updatedRate = model.ToRate();

            if (toEdit is not null)
            {
                toEdit.Update(updatedRate);
                await _companies.Update(company.Id.ToString(), company);


                //
                //Since i am updating the company model with rates, i need to also update every table that has RATES


                // var timeSum = await _timeSummaries.Find(x => x.CompanyId == company.Id);
                // foreach (var x in timeSum)
                // {
                //     IEnumerable<ClockData> clocks = timeSum.FirstOrDefault()!.Clocks;
                //     int num = 0;
                //     foreach(var clock in clocks)
                //     {
                //         if (clock.Rate.NameOfPosition == updatedRate.NameOfPosition)
                //         { 
                //             clocks.ToList()[num].Rate = updatedRate;
                //         }
                //         num++;
                //     }
                //     x.Clocks = clocks.ToList();

                //     await _timeSummaries.Update(x.Id.ToString(), x);
                //}

                //
                //End updating companyEmployee

            }
            else
                throw new HttpResponseException("Rate not found");

            return new Response<Rate>(toEdit, HttpStatusCode.Accepted);
        }

        private List<EmployeeTask> tasks = new();
        public async Task<Response> AddEmployeeToRosta(AddEmployeeToRostaModel model, string role, string? companyId = null)
        {
            // first of all the model must have RostaTasks
            if (model.RostaTasks is null || model.RostaTasks.Count == 0)
                throw new HttpResponseException("No rostaTasks where sent with your request!");
            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            var company = companyResponse.Data!;

            model = model.Sanitize();

            var employee = await companyEmployees.FindOne(x => x.CompanyId == company.Id && x.EmployeeId == model.EmployeeId);

            if (employee is null)
                throw new HttpResponseException($"{model.EmployeeId} does not work for {company.CompanyName}!");

            var shift = company.Shifts.Find(x => x.Name == model.ShiftName);
            if (shift is null)
                throw new HttpResponseException("Add a shift and rate for this position before adding an employee to timetable!");

            myrosta(model, shift, company, employee, 0);

            await companyEmployees.Update(employee.Id.ToString(), employee);
            //}

            return new Response(HttpStatusCode.OK, message: "Employee added to rosta.");
        }



        public async Task<Response> AddEmployeesToRosta(List<AddEmployeeToRostaModel> model, string role, string? companyId = null)
        {
            // the models should have atleast one item

            if (model is null || model.Count == 0)
                throw new HttpResponseException("No rosta was sent with your request!");


            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            var groupedModel = model.Select(x => x.Sanitize())
                .GroupBy(x => x.EmployeeId)
                .ToDictionary(x => x.Key, x => x.AsEnumerable());

            var company = companyResponse.Data!;

            var employeeIds = groupedModel.Keys;
            var queryBuilder = queryBuilderProvider.For<CompanyEmployee>();
            var query = queryBuilder.Eq(x => x.CompanyId, company.Id)
                .And<CompanyEmployee>(queryBuilder.In(x => x.EmployeeId, employeeIds));
            var employees = await companyEmployees.Find(query, 0, employeeIds.Count);

            if (!employees.Any())
                throw new HttpResponseException($"No _employees found!");

            var updatesBuilder = updateBuilderProvider.For<CompanyEmployee>();
            List<KeyValuePair<IQueryBuilder<CompanyEmployee>, IUpdateBuilder<CompanyEmployee>>> populatedUpdates = new();
            foreach (var empl in employees)
            {
                var updates = groupedModel[empl.EmployeeId];
                var rate = company.Rates.Find(x => x.NameOfPosition.EqualCaseInsesitive(empl.Position));
                // remove any existing values
                empl.WeekDays.Clear();

                if (rate is null)
                    throw new HttpResponseException($"Add rate for this position before adding an employee to timetable! Location {empl.Position}");

                foreach (var update in updates)
                {
                    var shift = company.Shifts.Find(x => x.Name == update.ShiftName);

                    if (shift is null)
                        throw new HttpResponseException($"No shift with name {update.ShiftName} found!");

                    foreach (var task in update.RostaTasks)
                    {
                        if (empl.WeekDays.ContainsKey(task.Weekday) is false)
                            empl.WeekDays[task.Weekday] = new();

                        var tasks = empl.WeekDays[task.Weekday];

                        // has the employee been already added to this shift

                        var exists = tasks.Any(x => x.Shift == shift.Id);

                        if (exists)
                            throw new HttpResponseException($"This employee has already been added to the shift '{shift.Name}' on the same day!");

                        tasks.Add(new EmployeeTask() { TaskName = task.TaskName, Rate = rate.Id, Shift = shift.Id, RateType = task.RateType });
                    }
                }

                var _dbUpdate = updatesBuilder.Set(x => x.WeekDays, empl.WeekDays);
                var _dbUpdateFilter = queryBuilder.New().Eq(x => x.Id, empl.Id);
                populatedUpdates.Add(new(_dbUpdateFilter, _dbUpdate));
            }

            var (updated, count) = await companyEmployees.Update(populatedUpdates);

            return updated
                ? new Response(HttpStatusCode.OK,
                message: count == populatedUpdates.Count
                ? "Employees added to rosta."
                : "We were not able to add some _employees to rosta.")
                : new Response(error: "Employees not added to rosta");
        }

        public async Task<Response> RemoveEmployeeFromRosta(RemoveEmployeeFromRostaModel model, string role, string? companyId = null)
        {
            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            var company = companyResponse.Data!;

            var employee = await companyEmployees.FindOne(x => x.CompanyId == company.Id && x.EmployeeId == model.EmployeeId);

            if (employee is null)
                return new Response(error: $@"There is no company employee with this id, working for {company.CompanyName}");

            if (employee.WeekDays.ContainsKey(model.Weekday) is false)
                return new Response(error: $@"This employee is not on a rosta for {model.Weekday}.");

            var tasks = employee.WeekDays[model.Weekday];

            tasks = tasks.Where(x => x.Id != ObjectId.Parse(model.RostaTaskId)).ToList();

            employee.WeekDays[model.Weekday] = tasks;

            await companyEmployees.Update(employee.Id.ToString(), employee);

            return new Response(HttpStatusCode.OK);
        }

        public async Task<Response> GetCustomization(string role, string? companyId = null)
        {
            var response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            Company company = companyResponse.Data!;

            IEnumerable<Customization> _customization = await customization.Find(x => x.CompanyId == company.Id);
            return new Response<IEnumerable<Customization>>(_customization);
        }

        public async Task<Response> RemovePaymentMethod(string deparment, string role, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            Company company = companyResponse.Data!;

            int index = company.SuppliersPaymentMethods.FindIndex(x => x == deparment);

            if (index > -1)
                company.SuppliersPaymentMethods.RemoveAt(index);
            else
                throw new HttpResponseException("Payment Method not found");

            await _companies.Update(companyId, company);

            return new Response(HttpStatusCode.OK, message: "Deleted");
        }

        public async Task<Response> GetLeaves(QueryLeaveModel model, string role)
        {
            Response response = await GetCompany(role, model.CompanyId);
            if (response is not Response<Company>)
                throw new HttpResponseException(response);

            BaseQueryBuilder<Leave> builder = (BaseQueryBuilder<Leave>)model.ToQuery(queryBuilderProvider);

            IEnumerable<Leave> result = await leaves.Find(builder);
            return new Response<IEnumerable<Leave>>(result);
        }

        private List<ClockData> leaveClock = new();
        private TimeSummary Summaries = new();
        private int jumpAfterEvery5Days = 5;

        public async Task<Response> UpdateLeave(string id, QueryLeaveModel model, string role, string? companyId = null)
        {
            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> companyResponse)
                throw new HttpResponseException(response);

            Company company = companyResponse.Data!;

            Leave leave = await leaves.FindById(id);
            if (leave is null)
                throw new HttpResponseException("No leave for provided id!");


            if (model.Status == LeaveStatus.Accepted)
            {
                TimeSummary times = await timeSummaries.FindOne(x => x.EmployeeId == leave.EmployeeSummary.EmployeeId && x.CompanyId == ObjectId.Parse(companyId));
                if (times is null)
                    throw new HttpResponseException("Leave can not be assigned to someone who has never worked with the company");

                //get companyId so i can know the position of this specific employee
                CompanyEmployee comEmployee = await companyEmployees.FindOne(x => x.CompanyId == company.Id && x.EmployeeId == leave.EmployeeSummary.EmployeeId);
                if (comEmployee is null)
                    throw new HttpResponseException("Something wrong with user's info");

                leaveClock = times.Clocks;
                if (times.EndDate < model.LeaveEndDate!.Value)
                    times.EndDate = model.LeaveEndDate!.Value; // update end date

                Rate userRates = company.Rates.FirstOrDefault(x => x.NameOfPosition == comEmployee.Position)!;
                Shift userShift = company.Shifts.FirstOrDefault()!;

                AddLeaveToClocks(times, model, userShift, userRates, 0);
                times.Clocks = leaveClock;

                await timeSummaries.Update(times.Id.ToString(), times);

            }

            leave = leave.Update(model);
            Leave result = await leaves.Update(id, leave);
            return new Response<Leave>(result);
        }



        private async Task<Response> GetCustom(string role, string? id = null)
        {
            async Task<Customization> C_Data()
                => await customization.FindById(id);

            Response response;
            switch (role)
            {
                case AuthRoles.Manager:
                    if (id is null)
                        response = new(error: "Customization list no found");
                    else
                        response = new Response<Customization>() { Data = await C_Data() };
                    break;

                case AuthRoles.Owner:
                    response = new Response<Customization>() { Data = await C_Data() };
                    break;
                default:
                    response = new Response(HttpStatusCode.Unauthorized, error: "You don't have access to this resource!");
                    break;
            }

            return response;

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

        private async Task<Response> GetSupplier(string role, string? id = null)
        {
            async Task<Supplier> Data()
                => await suppliers.FindById(id);

#nullable disable
            Response response;



            switch (role)
            {
                case AuthRoles.Manager:
                    if (id is null)
                        response = new(error: "Supplier id is not specified!");
                    else
                        response = new Response<Supplier>() { Data = await Data() };
                    break;

                case AuthRoles.Owner:
                    response = new Response<Supplier>() { Data = await Data() };
                    break;
                default:
                    response = new Response(HttpStatusCode.Unauthorized, error: "You don't have access to this resource!");
                    break;
            }

            return response;

        }

        private async void LeaveStoreFunc(string employeeId, ObjectId companyId)
        {
            var leave = leaveStore.FindOne(x => x.EmployeeId == employeeId && x.CompanyId == companyId).Result;
            if (leave is not null && leave.TimeStamp.Date != dateTimeProvider.Now.Date)
            {
                leave.AnnualLeaveDays = leave.AnnualLeaveDays + decimal.Divide(15, 260);// * 0.0576923076923077;
                leave.SickLeaveDays = leave.SickLeaveDays + decimal.Divide(1, 26); // * 0.0384615384615385)
                leave.TimeStamp = dateTimeProvider.Now.Date;

                leaveStore?.Update(leave.Id.ToString(), leave);
            }

            else if (leave is null)
            {
                var user = companyEmployees.FindOne(x => x.EmployeeId == employeeId).Result;
                leave = new LeaveStore
                {
                    AnnualLeaveDays = decimal.Divide(15, 260),// * 0.0576923076923077;
                    SickLeaveDays = decimal.Divide(1, 26), // * 0.0384615384615385)
                    FamilyLeaveDays = 3,
                    Name = user.Name,
                    Surname = user.Surname,
                    CompanyId = companyId,
                    EmployeeId = employeeId,
                    TimeStamp = dateTimeProvider.Now.Date
                };
                await leaveStore.Insert(leave);

            }


        }

        private void myrosta(AddEmployeeToRostaModel task, Shift shift, Company company, CompanyEmployee employee, int len)
        {
            if (task.RostaTasks.Count == len)
                return;

            if (employee.WeekDays.ContainsKey(task.RostaTasks[len].Weekday) is false)
                employee.WeekDays[(task.RostaTasks[len].Weekday)] = new();

            tasks = employee.WeekDays[(task.RostaTasks[len].Weekday)];
            var rate = company.Rates.Find(x => x.NameOfPosition.EqualCaseInsesitive(task.RostaTasks[len].TaskName));

            if (rate is null)
                throw new HttpResponseException("Add a rate for this position before adding an employee to timetable!");
            // has the employee been already added to this shift

            var exists = tasks.Any(x => x.Shift == shift.Id);

            if (exists)
                throw new HttpResponseException($"This employee has already been added to the shift '{task.ShiftName}' on the same day!");

            tasks.Add(new EmployeeTask()
            {
                TaskName = (task.RostaTasks[len].TaskName),
                AlocatedSite = task.RostaTasks[len].AlocatedSite,
                RateType = task.RostaTasks[len].RateType,
                Rate = rate.Id,
                Shift = shift.Id

            });
            myrosta(task, shift, company, employee, len + 1);
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
                var startTime = model.LeaveStartDate!.Value.AddDays(len).ToShortDateString() + " " + userShift.ShiftStartTime.ToString();
                var endTime = model.LeaveStartDate!.Value.AddDays(len).ToShortDateString() + " " + userShift.ShiftEndTime.ToString();
                var start = DateTime.Parse(startTime)!.AddMinutes(-120);
                var end = DateTime.Parse(endTime)!.AddMinutes(-120);


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


    }
}
