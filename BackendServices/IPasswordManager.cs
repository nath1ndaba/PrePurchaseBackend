using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendServices
{
    public interface IPasswordManager
    {
        Task<string> Hash(string password);
        Task<bool> IsMatch(string password, string hash);
    }
}
