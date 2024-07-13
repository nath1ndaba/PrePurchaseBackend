using BackendServices;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using BackendServices.Models.Payments;
using DnsClient.Internal;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.Payments;
using PrePurchase.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("api/v1")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = AuthPolicies.Company)]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class PaymentProcessingController : BaseController
    {
        private readonly IPayment _paymentActions;
        private readonly IRepository<TransactionModel> _transactions;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IRepository<Company> _companies;
        private readonly IRepository<History> _histories;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly IUpdateBuilderProvider _updateBuilderProvider;
        public ILogger<PaymentProcessingController> _loger;


        public PaymentProcessingController(IPayment actions,
            IRepository<TransactionModel> transactions,
            IRepository<CompanyEmployee> companyEmployees,
            IRepository<Company> companies,
            IRepository<History> histories,
            IQueryBuilderProvider queryBuilderProvider,
            IUpdateBuilderProvider updateBuilderProvider,
            ILogger<PaymentProcessingController> loger,
            IAuthContainerModel containerModel,
            IAuthService authService,
            IRepository<RefreshToken> refreshTokens)
            : base(containerModel, authService, refreshTokens)
        {
            _paymentActions = actions;
            _companyEmployees = companyEmployees;
            _companies = companies;
            _histories = histories;
            _transactions = transactions;
            _queryBuilderProvider = queryBuilderProvider;
            _updateBuilderProvider = updateBuilderProvider;
            _loger = loger;
        }

        [HttpPost("payroll")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(Response), 204)]
        [ProducesResponseType(typeof(Response), 402)]
        [ProducesResponseType(typeof(Response), 404)]
        public async Task<Response> ProcessPayroll([FromBody][Required] PayrollRequest request, [FromQuery] string companyId)
        {
            //_loger.LogInformation("getting company");
            Company company = await _companies.FindById(companyId);

            Response payoutBatches = await request.PrepareBatches(_histories, company, _queryBuilderProvider, _companyEmployees);

            _loger.LogInformation("payoutBatches Data: {@payoutBatches}", payoutBatches);
            if (payoutBatches is not Response<PayrollHelperModel> payoutRecords)
            {
                _loger.LogInformation("Data: {@payoutBatches}", payoutBatches);
                throw new HttpResponseException(payoutBatches);
            }

            //process payrolls payout
            Response<PayrollResponse> response = await _paymentActions.ProcessPayroll(payoutRecords.Data.PayOut);
            _loger.LogInformation($"After payment process: ");

            if (response is not Response<PayrollResponse> data)
            {
                var res = new Response(response.StatusCode, error: response.Message);
                throw new HttpResponseException(res);
            }

            //update to db

            await PayrollUpdateHandler(company.Id, data.Data, request, payoutRecords.Data.PayrollBatches, payoutRecords.Data.Employees);

            return new(HttpStatusCode.OK, "Success");
        }

        [HttpGet("payroll/history")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(Response), 404)]
        public async Task<Response<IEnumerable<History>>> GetPayrolHistory([FromQuery] string companyId)
        {
            //TO-Do: add paramaters so you can page or skip and limit
            //_loger.LogInformation("getting company");
            Company company = await _companies.FindById(companyId);

            //TO-Do: add paramaters so you can page or skip and limit
            IEnumerable<History> payrollBatch = await _histories.Find(x => x.CompanyId == company.Id && x.IsPaid == true, limit: 100);

            IEnumerable<string> employeesIds = payrollBatch.Select(x => x.EmployeeId);
            IQueryBuilder<CompanyEmployee> queryBuilder = _queryBuilderProvider.For<CompanyEmployee>();

            IQueryBuilder<CompanyEmployee> query = queryBuilder.Eq(x => x.CompanyId, ObjectId.Parse(companyId))
                .And<CompanyEmployee>(queryBuilder.In(x => x.EmployeeId, employeesIds));

            IEnumerable<CompanyEmployee> employees = await _companyEmployees.Find(query, limit: employeesIds.Count());

            payrollBatch = payrollBatch.Select(x =>
            {
                var employee = employees.FirstOrDefault(y => y.EmployeeId == x.EmployeeId);
                x.Name = employee?.Name;
                x.Surname = employee?.Surname;
                return x;
            });

            return new(payrollBatch);
        }

        [HttpPost("checkout")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(Response), 404)]
        public async Task<Response> RequestCheckout([FromBody][Required] PaymentModel model)
        {
            return await _paymentActions.RequestCheckout(model);
        }

        [HttpPost("paymentStatus")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(Response), 404)]
        public async Task<Response> RequestPaymentStatus([FromBody][Required] PaymentStatusRequest request)
        {
            _loger.LogInformation("Payment request: {request}", request);
            var res = await _paymentActions.RequestPaymentStatus(request.id);
            _loger.LogInformation("Payment request Status: {res}", res);

            if (res is not Response<TransactionModel> response)
                throw new HttpResponseException(res);

            var transaction = response.Data!;

            var companyId = GetId(null);
            var company = await _companies.FindById(companyId);

            var transactionTask = _transactions.Insert(transaction);

            company.AccountBalance.CurrentBalance += transaction.Amount;

            var updateTask = _companies.Update(companyId, company);

            await Task.WhenAll(transactionTask, updateTask);
            _loger.LogInformation("Company Update: {updateTask}", updateTask);

            var _queryBuilder = _queryBuilderProvider.For<History>();

            //Continue With Processing Payroll
            var paymentRequest = new PayrollRequest(request.Service, request.ServiceType, request.batchCode);

            var payoutBatches = await paymentRequest.PrepareBatches(_histories, company, _queryBuilderProvider, _companyEmployees);

            if (payoutBatches is not Response<PayrollHelperModel> payoutRecords || payoutBatches.StatusCode != HttpStatusCode.OK)
                throw new HttpResponseException(payoutBatches);

            //process payrolls payout
            var payResponse = await _paymentActions.ProcessPayroll(payoutRecords.Data.PayOut);
            _loger.LogInformation($"After payment process: ");

            if (payResponse is not Response<PayrollResponse> data)
            {
                var resError = new Response(response.StatusCode, error: response.Message);
                throw new HttpResponseException(resError);
            }

            //update to db

            await PayrollUpdateHandler(company.Id, data.Data, paymentRequest, payoutRecords.Data.PayrollBatches, payoutRecords.Data.Employees);

            return new Response<TransactionModel>(transaction, HttpStatusCode.OK, response.Message);
        }

        private async Task PayrollUpdateHandler(ObjectId CompanyId, PayrollResponse data, PayrollRequest request, IEnumerable<History> payrollBatches, IEnumerable<CompanyEmployee> employees)
        {
            var _queryBuilder = _queryBuilderProvider.For<History>();
            //update to db
            List<History> _payrollBatches = payrollBatches.ToList();
            List<KeyValuePair<IQueryBuilder<History>, IUpdateBuilder<History>>> updates = new(_payrollBatches.Count);

            TransactionModel _transaction = new(
                    data.BatchCode.ToString(),
                    data.PaidAmount, Currency.ZAR,
                    request.ServiceType == ServiceType.OneDayOnly ? "1Day" : request.ServiceType.ToString(),
                    PaymentMethod.Payroll, DateTime.UtcNow.ToUniversalTime(), data.BatchPayoutFee);

            _transaction.CompanyId = CompanyId;

            //update IsPaid value in user payroll history

            foreach (var _batch in _payrollBatches)
            {
                var batch_updateBuilder = _updateBuilderProvider.For<History>();
                var batch_queryBuilder = _queryBuilderProvider.For<History>();

                if (data.UnPaids is { Count: > 0 })
                {
                    // only if there where unpaid
                    var _employee = employees.FirstOrDefault(y => y.EmployeeId == _batch.EmployeeId);
                    var _foundInUnPaid = data.UnPaids.FindIndex(a => a.AccountNumber == _employee.BankAccountInfo.AccountNumber);

                    batch_updateBuilder = batch_updateBuilder
                        .Set(x => x.IsPaid, _foundInUnPaid > -1 ? false : true)
                        .Set(x => x.TransactionId, _foundInUnPaid > -1 ? null : _transaction.Id);
                }
                else
                {
                    batch_updateBuilder = batch_updateBuilder
                        .Set(x => x.IsPaid, true)
                        .Set(x => x.TransactionId, _transaction.Id);
                }

                var batch_query = _queryBuilder.Eq(x => x.Id, _batch.Id);
                updates.Add(new(batch_query, batch_updateBuilder));
            }
            _loger.LogInformation("Saving");
            await _transactions.Insert(_transaction);
            _loger.LogInformation("Updating");
            await _histories.Update(updates);
            _loger.LogInformation("Success");
        }
        private string GetRole()
        {
            var accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }
        public async Task<Response> GetCompany(string role, string id = null)
        {
            async Task<Company> Data()
                => await _companies.FindById(id);

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
        private string? GetId(string? companyId = null)
        {
            var role = GetRole();

            if (role == AuthRoles.Owner)
                return Id;

            return companyId;
        }

        private void RequireCompanyId(string companyId)
        {
            var role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(companyId))
                throw new HttpResponseException(new Response(error: "Company id is not specified!")); ;

        }

#nullable disable

    }
}
