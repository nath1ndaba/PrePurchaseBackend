
using System.Threading.Tasks;

namespace BackendServices
{
    public interface IEmployeeIdGenerator
    {
        string GetNew();
        Task<string> GetNewAsync()
         => Task.FromResult(GetNew());
    }
}
