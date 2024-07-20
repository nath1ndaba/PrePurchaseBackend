using System.Threading.Tasks;
using BackendServices.Models;
using MongoDB.Bson;

namespace BackendServices.Actions.PrePurchase
{
    public interface IResidentActions
    {
        Task<Response> RegisterResident(AdminRegisterModel model, ObjectId createdBy);
        Task<Response> UpdateResident(AdminRegisterModel model, ObjectId createdBy);
        Task<Response> GetResident(string adminId);
        Task<Response> GetResidents();
        Task<Response> ArchiveResident(string adminId, string updatedBy);
        Task<Response> RestoreResident(string adminId, string updatedBy);
    }
}