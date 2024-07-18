using System.Threading.Tasks;
using BackendServices.Models;
using MongoDB.Bson;

namespace BackendServices.Actions.PrePurchase.AdminPortal
{
    public interface IAdminActions
    {
        Task<Response> Register(AdminRegisterModel model, ObjectId createdBy);
        Task<Response> GetAdmin(string adminId);
        Task<Response> GetAdmins();
        Task<Response> ArchiveAdmin(string adminId, string updatedBy);
        Task<Response> RestoreAdmin(string adminId, string updatedBy);
    }
}