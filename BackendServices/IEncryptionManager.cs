using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendServices
{
    public interface IEncryptionManager
    {
        Task<(string key, string iv, string encryptedMessage)> Encrypt(string message);
        Task<string> Decrypt(string key, string iv, string encryptedMessage);
    }
}
