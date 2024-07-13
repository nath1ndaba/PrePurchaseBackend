using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackendServices.Models;
using PrePurchase.Models.Requests;
using BackendServices.Exceptions;
using BackendServices.Models.Payments;
using BackendServices;
using PrePurchase.Models;
using System.Net;

namespace Infrastructure.Helpers
{
    public static class PayrollHelpers
    {
        public static async Task<Response> PrepareBatches(this PayrollRequest request, IRepository<History> histories,
            Company company, IQueryBuilderProvider queryBuilderProvider, IRepository<CompanyEmployee> companyEmployees)
        {
            var _queryBuilder = queryBuilderProvider.For<History>();

            //group the _histories by ids
            //Continue With Processing Payroll
            //ensure the _employees belong to this company
            var _query = _queryBuilder.Eq(x => x.BatchCode, request.BatchCode)
                .And(x => x.CompanyId, company.Id)
                .And(x => x.IsPaid, false);
            var payrollBatches = await histories.Find(_query);
            var _payrollIds = payrollBatches.Select(x => x.EmployeeId).ToList();

            //check if the payroll batch exist
            if (!payrollBatches.Any())
                return new Response(HttpStatusCode.NoContent, error: "No payroll batch found!");

            var employeesIds = payrollBatches.Select(x => x.EmployeeId).ToList();
            var queryBuilder = queryBuilderProvider.For<CompanyEmployee>();

            var query = queryBuilder.Eq(x => x.CompanyId, company.Id)
                .And<CompanyEmployee>(queryBuilder.In(x => x.EmployeeId, employeesIds));

            var employees = await companyEmployees.Find(query, limit: employeesIds.Count).ToDictionaryAsync(x => x.EmployeeId);

            if (!employees.Any())
                return new Response(HttpStatusCode.NoContent, error: "No _employees found!");

            //check if the _employees have bank accounts
            if (employees.Values.Any(x => x.BankAccountInfo == null))
                return new Response(HttpStatusCode.BadRequest, error: "Some of the _employees seems to not have banking information, fix this and try again later.");

            //Check that they have a balance > more than the payout amount
            //If they have insuficient funds send them to checkout so that they can top up
            if (company.AccountBalance.CurrentBalance < payrollBatches.Sum(x => x.TotalAmount))
                return new Response(HttpStatusCode.PaymentRequired, data: company.AccountBalance.CurrentBalance, error: "You don't have enough funds in your Stella account balance. Please top up your account and try again.");

            //get _employees bank accounts info to process payment
            var payoutRecords = new PayOutModel()
            {
                //----production codes-----
                Payments = payrollBatches.Where(y => y.TotalNet > 0).Select(x =>
                {
                    var _employee = employees[x.EmployeeId];

                    var payment = new PayOutPaymentModel()
                    {
                        Surname = _employee.BankAccountInfo.AccountHolder,
                        AccountNumber = _employee.BankAccountInfo.AccountNumber!.Value,
                        AccountType = 0,
                        BranchCode = _employee.BankAccountInfo.BranchCode!.Value,
                        FileAmount = x.TotalNet,
                        AmountMultiplier = 1,
                        CustomerCode = _employee.EmployeeId,
                    };
                    return payment;
                }),
                Reference = $"{company.CompanyName} {request.Service}"
            };

            var _model = new PayrollHelperModel()
            {
                PayOut = payoutRecords,
                PayrollBatches = payrollBatches
            };

            return new Response<PayrollHelperModel>(_model);
        }
    }

    public class PayrollHelperModel
    {
        public PayOutModel PayOut { get; set; }
        public IEnumerable<History> PayrollBatches { get; set; }
        public IEnumerable<CompanyEmployee> Employees { get; set; }
    }
}
