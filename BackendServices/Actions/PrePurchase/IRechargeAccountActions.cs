using BackendServices.Models;
using PrePurchase.Models.PrePurchase;
using System.Threading.Tasks;

namespace BackendServices.Actions.PrePurchase
{
    public interface IRechargeAccountActions
    {
        Task<Response> GetRecharge(string id, string userId = null);
        Task<Response> GetRecharges(string userId);
        Task<Response> RechargeAccount(RechargeDto model, string createdBy, string userId = null);
        Task<Response> UpdateUserAccountBalance(decimal amount, string updatedBy, string userId = null);
        Task<Response> GetUserAccountBalance(string userId);
        Task<Response> GetDashboardData(string userId);

    }
}