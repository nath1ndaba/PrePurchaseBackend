using BackendServices.Models;
using PrePurchase.Models.Inventory;
using System.Threading.Tasks;

namespace BackendServices.Actions.Inventory
{
    public interface IStockCountsActions
    {
        Task<Response> AddStockCount(string createdBy, string updatedBy, StockCount model, string role, string companyId = null);
        Task<Response> GetStockCounts(string role, string companyId = null);
        Task<Response> UpdateStockCount(string updatedBy, StockCount model, string role, string companyId = null);
        Task<Response> SoftDeleteStockCount(string updatedBy, string id, string role, string companyId = null);
        Task<Response> GetStockCount(string id, string role, string companyId = null);
    }
}
