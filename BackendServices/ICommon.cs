using System.Threading.Tasks;

namespace BackendServices
{
    public interface ICommon
    {
        Task<T> ValidateCompany<T>(string role, string companyId = null) where T : class;
    }
}
