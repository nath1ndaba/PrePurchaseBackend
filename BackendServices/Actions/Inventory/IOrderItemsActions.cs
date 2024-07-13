using BackendServices.Models;
using PrePurchase.Models.Inventory;
using System.Threading.Tasks;

namespace BackendServices.Actions.Inventory
{
    public interface IOrderItemsActions
    {
        Task<Response> AddOrderItem(string createdBy, string updatedBy, OrderItem model, string role, string companyId = null);
        Task<Response> GetOrderItems(string role, string companyId = null);
        Task<Response> UpdateOrderItem(string updatedBy, OrderItem model, string role, string companyId = null);
        Task<Response> SoftDeleteOrderItem(string updatedBy, string id, string role, string companyId = null);
        Task<Response> GetOrderItem(string id, string role, string companyId = null);
    }
}
