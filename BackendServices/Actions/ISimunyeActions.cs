using BackendServices.Models;
using SOG.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions
{
    /// <summary>
    /// This interface defines the contract of the actions a company is able to perform on our system
    /// </summary>
#nullable enable
    public interface ISimunyeActions
    {

        Task<Response> AddDepartments(string createdBy, string updatedBy, SOGDepartments model);
        Task<Response> AddPositions(string createdBy, string updatedBy, SOGPositions model);
        Task<Response> AddMember(string createdBy, string updatedBy, SOGMembers model);
        Task<Response> AddIsifo(string createdBy, string updatedBy, SOGIzifo model, string role, string? memberId = null);
        Task<Response> AddContribution(string createdBy, string updatedBy, SOGContribution contributer, string role, string? memberId = null);

        Task<Response> GetDepartments(string role, string? MemberId = null);
        Task<Response> GetPositions(string role, string? MemberId = null);
        Task<Response> GetMembers(string role, string? MemberId = null);
        Task<Response> GetIzifo(string role, string? MemberId = null);
        Task<Response> GetContributions(string role, string isifoId, string? MemberId = null);


        Task<Response> UpdateDepartment(string updatedBy, SOGDepartments model);
        Task<Response> UpdatePosition(string updatedBy, SOGPositions model);
        Task<Response> UpdateMember(string updatedBy, SOGMembers model);
        Task<Response> UpdateContribution(string updatedBy, SOGContribution model);




        Task<Response> SoftDeleteDepartment(string updatedBy, string id);
        Task<Response> SoftDeletePosition(string updatedBy, string id);
        Task<Response> SoftDeleteMember(string updatedBy, string id);
        Task<Response> SoftDeleteIsifo(string updatedBy, string id);
        Task<Response> SoftDeleteContribution(string updatedBy, string id);


        Task<Response> Login(MemberLogin model);

        Task<Response> Get(string role, string id);
    }

#nullable disable
}
