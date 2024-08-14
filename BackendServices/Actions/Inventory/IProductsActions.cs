using BackendServices.Models;
using BackendServices.Models.Inventory;
using PrePurchase.Models.Inventory;
using System.Threading.Tasks;

namespace BackendServices.Actions.Inventory
{
    public interface IProductsActions
    {
        Task<Response> GetProducts(string shopId);
        Task<Response> GetProductsForCategory(string categoryId, string shopId);
        Task<Response> AddProduct(string createdBy, string updatedBy, ProductDto model, string shopId = null);
        Task<Response> UpdateProduct(string updatedBy, Product product, string shopId = null);
        Task<Response> GetProduct(string id, string shopId = null);
        Task<Response> SoftDeleteProduct(string updatedBy, string id, string shopId = null);
    }
}
