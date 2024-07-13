using BackendServices.Models;
using PrePurchase.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface IUserActions
    {
        Task<Response> RegisterUser(User model);
        Task<Response> GetUser(string email);





    }
}
