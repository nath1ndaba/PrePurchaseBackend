using BackendServices.Models;
using PrePurchase.Models.Inventory;
using System.Threading.Tasks;

namespace BackendServices.Actions.Inventory
{
    public interface ISuppliersActions
    {
        Task<Response> AddSupplier(string createdBy, string updatedBy, Supplier model, string role, string companyId = null);
        Task<Response> GetSuppliers(string role, string companyId = null);
        Task<Response> UpdateSupplier(string updatedBy, Supplier model, string role, string companyId = null);
        Task<Response> SoftDeleteSupplier(string updatedBy, string id, string role, string companyId = null);
        Task<Response> GetSupplier(string id, string role, string companyId = null);
    }
}
