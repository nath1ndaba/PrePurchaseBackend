using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using PrePurchase.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.PrePurchase
{
    public interface ICashToItemActions
    {
        Task<Response> GetTopNearbyShops(ResidentLocation residentLocation, int topN);

        Task<Response> ConvertCashToItem(CashToItemDto model, string createdBy, string userId = null);
        Task<Response> GetCashToItem(string id, string userId = null);
        Task<Response> GetCashToItems(string userId);
        Task<Response> GetCashToItemForDashboard(string userId);
        Task<Response> UpdateCashToItem(CashToItemDto cashToItem, string updatedBy, string userId = null);
        Task<Response> UndoCashToItem(string id, string userId);
    }
}