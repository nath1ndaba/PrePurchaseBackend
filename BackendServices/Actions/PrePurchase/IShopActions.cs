using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace BackendServices.Actions.PrePurchase
{
    public interface IShopActions
    {
        Task<Response> RegisterShop(ShopDto model, string createdBy);
        Task<Response> UpdateShop(ShopDto model, string createdBy);
        Task<Response> GetShop(string adminId);
        Task<Response> GetShops();
        Task<Response> ArchiveShop(string adminId, string updatedBy);
        Task<Response> RestoreShop(string adminId, string updatedBy);
    }
}