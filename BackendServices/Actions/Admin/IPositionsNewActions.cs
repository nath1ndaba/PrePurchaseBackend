using BackendServices.Models;
using PrePurchase.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface IPositionsNewActions
    {
        Task<Response> AddPosition(string createdBy, string updatedBy, CompanyPositions model, string role, string companyid = null);
        Task<Response> UpdatePosition(string updatedBy, CompanyPositions model, string role, string companyid = null);
        Task<Response> GetPositions(string role, string companyid = null);
        Task<Response> GetPosition(string id, string role, string companyid = null);
        Task<Response> SoftDeletePosition(string updatedBy, string id, string role, string companyid = null);
    }
}
