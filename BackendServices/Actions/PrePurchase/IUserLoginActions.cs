using BackendServices.Models;
using PrePurchase.Models.PrePurchase;
using System.Threading.Tasks;

namespace BackendServices.Actions.PrePurchase
{
    public interface IUserLoginActions
    {
        Task<UserLoginResponse> UserLogin(LoginModel model);
    }
}