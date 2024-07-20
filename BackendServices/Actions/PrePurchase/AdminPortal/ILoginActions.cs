using System.Threading.Tasks;
using BackendServices.Models;
using PrePurchase.Models;

namespace BackendServices.Actions.PrePurchase.AdminPortal
{
    public interface ILoginActions
    {
        Task<LoginResponse> Login(LoginModel model);
    }
}