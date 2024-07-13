using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.Models;
using Infrastructure.Helpers;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
    public class LoansActions : ILoansActions
    {
        public async Task<Response> AddnewLoan(string createdby, string updatedby, string employeeId, RequestLoanModel model, string role, string? companyid = null)
        {
            try
            {

                if (ObjectId.TryParse(companyid, out var _companyId) is false)
                    throw new HttpResponseException("Invalid companyId!!");
                Company company = await _common.ValidateCompany(role, companyid);

                // check that this employee works for the company they are requesting a loan from
                employeeId = employeeId.ToLowerInvariant();


                if (company?.IsLoanActive is true)
                {
                    List<Task> tasks = new();
                    Task<CompanyEmployee> companyEmployeeTask = _companyEmployees.FindOne(x => x.EmployeeId.ToLowerInvariant() == employeeId && x.CompanyId == _companyId);
                    Task<EmployeeDetails> employeeDetailsTask = _employeeDetails.FindOne(x => x.EmployeeId.ToLowerInvariant() == employeeId);
                    tasks.Add(companyEmployeeTask);
                    tasks.Add(employeeDetailsTask);
                    Task.WaitAll(tasks.ToArray());

                    CompanyEmployee? companyEmployee = companyEmployeeTask.Result ?? throw new HttpResponseException("You do not work for this company!!");
                    EmployeeDetails? employeeDetails = employeeDetailsTask.Result;
                    EmployeeSummary employeeSummary = new()
                    {
                        Id = employeeDetails.Id,
                        Name = employeeDetails.Name,
                        Surname = employeeDetails.Surname,
                        EmployeeId = employeeDetails.EmployeeId
                    };

                    DateTime expectedLastPaymentDate = DateTime.UtcNow.AddMonths(decimal.ToInt32(model.LoanDurationInMonths));
                    Loan loan = new()
                    {
                        CreatedBy = ObjectId.Parse(createdby),
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = ObjectId.Parse(updatedby),
                        UpdatedDate = DateTime.UtcNow,
                        DeletedIndicator = false,
                        LoanAmount = model.LoanAmount,
                        LoanDurationInMonths = model.LoanDurationInMonths,
                        LastPaymentDate = expectedLastPaymentDate,
                        Reason = model.Reason,
                        CompanyId = _companyId,
                        EmployeeSummary = employeeSummary,
                        TimeStamp = _dateTimeProvider.Now,
                        Department = companyEmployee.Department,
                        Position = companyEmployee.Position
                    };

                    await _loans.Insert(loan);
                    return new Response(HttpStatusCode.Accepted, message: $"loan has been applied");
                }
                else
                {
                    return new Response(HttpStatusCode.Forbidden, message: $"{company?.CompanyName} does not offer loans");
                }
            }
            catch { throw; }
            finally { }
        }

        public async Task<Response> GetLoans(QueryLoanModel model, string role)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, model.CompanyId);

                if (company!.IsLoanActive is true)
                {
                    BaseQueryBuilder<Loan> builder = (BaseQueryBuilder<Loan>)model.ToQuery(_queryBuilderProvider);
                    IEnumerable<Loan> result = await _loans.Find(builder);
                    return new Response<IEnumerable<Loan>>(result);
                }
                else
                {
                    throw new HttpResponseException("Company does not offer loans");
                }
            }
            catch { throw; }
            finally { }
        }


        public async Task<Response> UpdateLoan(string updatedBy, string id, CompanyUpdateLoan model, string role, string? companyid = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyid);

                if (company!.IsLoanActive is true)
                {
                    Loan loan = await _loans.FindById(id);

                    if (loan is null)
                        return new Response(error: "No loan for provided id!");

                    loan = loan.Update(model, _dateTimeProvider, updatedBy);
                    Loan result = await _loans.Update(id, loan);
                    // return new Response<Loan>(result);
                    return new Response(HttpStatusCode.Accepted, message: $"loan updated");


                }
                else
                {
                    return new Response(HttpStatusCode.Forbidden, message: $"{company.CompanyName} does not offer loans");
                }
            }
            catch { throw; }
            finally { }
        }


        public LoansActions(IRepository<Loan> loans, IRepository<CompanyEmployee> companyEmployees, IRepository<EmployeeDetails> employeeDetails, ICommon common, IQueryBuilderProvider queryBuilderProvider, IDateTimeProvider dateTimeProvider)
        {
            _loans = loans;
            _companyEmployees = companyEmployees;
            _employeeDetails = employeeDetails;
            _common = common;
            _queryBuilderProvider = queryBuilderProvider;
            _dateTimeProvider = dateTimeProvider;
        }
        private readonly IRepository<Loan> _loans;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IRepository<EmployeeDetails> _employeeDetails;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICommon _common;

    }
}
