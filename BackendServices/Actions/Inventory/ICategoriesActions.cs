using BackendServices.Models;
using PrePurchase.Models.Inventory;
using System.Threading.Tasks;

namespace BackendServices.Actions.Inventory
{
    public interface ICategoriesActions
    {
        Task<Response> AddCategory(string createdBy, string updatedBy, Category model, string role, string companyId = null);
        Task<Response> GetCategories(string role, string companyId = null);
        Task<Response> UpdateCategory(string updatedBy, Category model, string role, string companyId = null);
        Task<Response> SoftDeleteCategory(string updatedBy, string id, string role, string companyId = null);
        Task<Response> GetCategory(string id, string role, string companyId = null);
    }
}
