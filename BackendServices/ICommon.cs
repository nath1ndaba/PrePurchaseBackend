using System.Threading.Tasks;

namespace BackendServices
{
    public interface ICommon
    {
        Task<T> ValidateOwner<T>(string role, string companyId = null) where T : class;
    }
}
