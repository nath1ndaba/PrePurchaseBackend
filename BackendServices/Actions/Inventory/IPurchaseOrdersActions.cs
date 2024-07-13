using BackendServices.Models;
using PrePurchase.Models.Inventory;
using System.Threading.Tasks;

namespace BackendServices.Actions.Inventory
{
    public interface IPurchaseOrdersActions
    {
        Task<Response> AddPurchaseOrder(string createdBy, string updatedBy, PurchaseOrder model, string role, string companyId = null);
        Task<Response> GetPurchaseOrders(string role, string companyId = null);
        Task<Response> UpdatePurchaseOrder(string updatedBy, PurchaseOrder model, string role, string companyId = null);
        Task<Response> SoftDeletePurchaseOrder(string updatedBy, string id, string role, string companyId = null);
        Task<Response> GetPurchaseOrder(string id, string role, string companyId = null);
    }
}
