using BackendServices.Models;
using System.Threading.Tasks;
using PrePurchase.Models;

namespace BackendServices.Actions.Admin
{
    public interface IDepartmentsNewActions
    {
        Task<Response> AddDepartments(string createdBy, string updatedBy, CompanyDepartments model, string role, string companyid = null);
        Task<Response> GetDepartments(string role, string companyid = null);
        Task<Response> UpdateDepartment(string updatedBy, CompanyDepartments model, string role, string companyid = null);
        Task<Response> SoftDeleteDepartment(string updatedBy, string id, string role, string companyid = null);
        Task<Response> GetDepartment(string id, string role, string companyid = null);
    }
}
