using BackendServices.Models;
using PrePurchase.Models;
using PrePurchase.Models.LeaveDays;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BackendServices.Actions
{
    /// <summary>
    /// This interface defines the contract of the actions an employee is able to perform on our system
    /// </summary>
    public interface IEmployeeActions
    {
        Task<Response> GetDetailedAds();
        Task<Response> Login(EmployeeLoginModel model);
        Task<Response> ChangePassword(string id, ChangePasswordModel model);
        Task<Response> FindById(string id);
        Task<Response> FindByEmployeeId(string employeeId);
        Task<Response> ClockInOut(ClockInAndOutData clockInOutData, string employeeId, string employeeDetailsId);
        Task<Response> GetClockedInStatus(string companyId, string employeeDetailsId);
        Task<Response> LoanById(string employeeDetailsId, string id);
        Task<Response> RequestLoan(string createdBy, string updatedBy, RequestLoanModel model, string employeeId, string companyId);
        Task<Response> RemoveLoan(string companyId, string loanId, string employeeId);
        Task<Response> Loan(Expression<Func<Loan, bool>> filter);
        Task<Response> LoansByEmployeeDetailsId(string companyId, string id, int skip = 0, int limit = 100);
        Task<Response> Loans(Expression<Func<Loan, bool>> filter, int skip = 0, int limit = 100);
        Task<Response> LeaveById(string employeeDetailsId, string id);
        Task<Response> LeaveStoreByEmployeeId(string companyId, string id);
        Task<Response> RequestLeave(string createdBy, string updatedBy, LeaveStatus status, RequestLeaveModel model, string employeeId, string companyId);
        Task<Response> RemoveLeave(string updatedBy, string companyId, string leaveId, string employeeId);
        Task<Response> Leave(Expression<Func<Leave, bool>> filter);
        Task<Response> Rosta(string employeeId, string companyId);
        Task<Response> LeavesByEmployeeDetailsId(string companyId, string id, int skip = 0, int limit = 100);
        Task<Response> Leaves(Expression<Func<Leave, bool>> filter, int skip = 0, int limit = 100);
        Task<Response> TimeSummaryById(string employeeDetailsId, string id);
        Task<Response> TimeSummary(Expression<Func<TimeSummary, bool>> filter);
        Task<Response> TimeSummariesByEmployeeDetailsId(string id, int skip = 0, int limit = 100);
        Task<Response> MobileWalletByEmployeeDetailsId(string id, string companyId);
        Task<Response> TimeSummaries(Expression<Func<TimeSummary, bool>> filter, int skip = 0, int limit = 100);
        Task<Response> TimeSummariesForRange(string companyId, DateTime start, DateTime end, int skip = 0, int limit = 100);
        Task<Response> HistoryById(string employeeDetailsId, string id);
        Task<Response> History(Expression<Func<History, bool>> filter);
        Task<Response> HistoriesByEmployeeDetailsId(string employeeDetailsId, string companyId, int skip = 0, int limit = 100);
        Task<Response> Histories(Expression<Func<History, bool>> filter, int skip = 0, int limit = 100);
        Task<Response> CompanyProfiles(string employeeId);
    }
}
