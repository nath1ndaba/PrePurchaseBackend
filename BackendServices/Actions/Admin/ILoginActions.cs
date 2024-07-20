using BackendServices.Models;
using PrePurchase.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface ILoginActions
    {
        Task<LoginResponse> Login(LoginModel model);
    }
}
