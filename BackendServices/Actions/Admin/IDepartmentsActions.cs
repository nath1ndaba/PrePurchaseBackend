using BackendServices.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface IDepartmentsActions
    {
        Task<Response> AddDepartment(string deparment, string role, string companyId = null);
        Task<Response> GetDepartments(string role, string companyId = null);
        Task<Response> RemoveDepartment(string deparment, string role, string companyId = null);
    }
}
