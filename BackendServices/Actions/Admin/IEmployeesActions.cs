using BackendServices.Models;
using PrePurchase.Models;
using PrePurchase.Models.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface IEmployeesActions
    {
        Task<Response> GetEmployees(string role, string companyId = null);
        Task<Response> GetArchivedEmployees(string role, string companyId = null);
        Task<Response> UnArchiveEmployee(string employeeId, string updatedBy, string companyId = null);
        Task<Response> GetEmployeesByDepartment(string department, string role, string companyId = null);
        Task<Response> ResetEmployeePassword(string employeeDetailsId, string updatedBy);
        Task<Response> AddEmployee(AddEmployeeModel model, string createdBy, string updatedBy, string role, string companyId = null);
        Task<Response> ImportEmployees(List<AddEmployeeModel> model, string createdBy, string updatedBy, string role, string companyId = null);
        Task<Response> AddExistingEmployeeToNewCompany(AddExistingEmployeeToNewCompany model, string companyId = null);
        Task<Response> UpdateEmployee(string employeeId, UpdateEmployeeRequest model, string updatedBy, string role, string companyId = null);
        Task<Response> ResetEmployeeDevice(string employeeId, string updatedBy);
        Task<Response> SoftDeleteEmployee(string deletedBy, string id, string companyId);



    }
}
