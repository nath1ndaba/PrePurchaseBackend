using BackendServices.Models;
using PrePurchase.Models.Inventory;
using System.Threading.Tasks;

namespace BackendServices.Actions.Inventory
{
    public interface IProductsActions
    {
        Task<Response> AddProduct(string createdBy, string updatedBy, Product model, string role, string shopId = null);
        Task<Response> GetProduct(string id, string role, string companyId = null);
        Task<Response> GetProducts(string role, string shopId = null);
        Task<Response> GetProductsForCategory(string role, string categoryId, string shopId = null);
        Task<Response> UpdateProduct(string updatedBy, Product model, string role, string companyId = null);
        Task<Response> SoftDeleteProduct(string updatedBy, string id, string role, string companyId = null);
    }
}
