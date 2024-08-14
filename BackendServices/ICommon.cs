using System.Threading.Tasks;

namespace BackendServices
{
    public interface ICommon
    {
        Task<T> ValidateOwner<T>(string role = null, string id = null) where T : class;
        Task<T> ValidateOwner<T>(string id) where T : class;
    }
}
