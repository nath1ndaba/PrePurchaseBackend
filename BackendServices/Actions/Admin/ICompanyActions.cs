using BackendServices.Models;
using PrePurchase.Models;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface ICompanyActions
    {
        Task<Response> GetCompany(string role, string id);
        Task<Response> Register(AdminRegisterModel model);
        Task<Response> Update(CompanyUpdateModel update, string role, string companyId = null);
        Task<Response> AddCompanySites(Location location, string role, string companyId = null);

        Task<Response> AddPosition(string position, string role, string companyId = null);
        Task<Response> RemovePosition(string position, string role, string companyId = null);

        Task<Response> AddShift(ShiftModel shift, string role, string companyId = null);
        Task<Response> RemoveShift(string id, string role, string companyId = null);
        Task<Response> UpdateShift(string id, ShiftModel shift, string role, string companyId = null);

        Task<Response> AddRate(RateModel rate, string role, string companyId = null);
        Task<Response> RemoveRate(string id, string role, string companyId = null);
        Task<Response> UpdateRate(string id, RateModel rate, string role, string companyId = null);


        Task<Response> AddDeductions(DeductionModel model, string role, string companyId = null);
        Task<Response> UpdateDeduction(string id, DeductionModel model, string role, string companyId = null);
        Task<Response> RemoveDeduction(string id, string role, string companyId = null);





    }
}
