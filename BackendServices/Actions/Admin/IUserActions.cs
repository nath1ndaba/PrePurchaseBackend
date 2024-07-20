using BackendServices.Models;
using PrePurchase.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface IDiscontinuedUserActions
    {
        Task<Response> RegisterUser(DiscontinuedUser model);
        Task<Response> GetUser(string email);





    }
}
