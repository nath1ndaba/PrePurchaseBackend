using BackendServices.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface ILoansActions
    {
        Task<Response> GetLoans(QueryLoanModel model, string role);
        Task<Response> UpdateLoan(string updatedBy, string id, CompanyUpdateLoan model, string role, string companyId = null);
        Task<Response> AddnewLoan(string createdby, string updatedby, string employeeId, RequestLoanModel model, string role, string companyId = null);
    }
}
