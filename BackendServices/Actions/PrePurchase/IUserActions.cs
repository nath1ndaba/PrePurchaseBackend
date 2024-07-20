using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace BackendServices.Actions.PrePurchase
{
    public interface IUserActions
    {
        Task<Response> RegisterUser(UserDto model, string createdBy);
        Task<Response> UpdateUser(UserDto model, string updatedBy);
        Task<Response> GetUser(string userId);
        Task<Response> GetUsers();
        Task<Response> ArchiveUser(string userId, string updatedBy);
        Task<Response> RestoreUser(string userId, string updatedBy);
    }
}