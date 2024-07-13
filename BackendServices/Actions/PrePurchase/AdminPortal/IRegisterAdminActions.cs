using System.Threading.Tasks;
using BackendServices.Models;
using MongoDB.Bson;

namespace BackendServices.Actions.PrePurchase.AdminPortal
{
    public interface IRegisterAdminActions
    {
        Task<Response> Register(AdminRegisterModel model, ObjectId createdBy);
    }
}