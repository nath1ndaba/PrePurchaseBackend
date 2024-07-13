using BackendServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal class PasswordManager : IPasswordManager
    {
        private readonly int DegreesOfParallelism = PrePurchaseConfig.ARGON_PARALLELISM_FACTOR;
        private readonly int Iterations = PrePurchaseConfig.ARGON_ITERATIONS;
        private readonly int MemorySize = PrePurchaseConfig.ARGON_MEMORY_COST;
        private readonly int HashLength = PrePurchaseConfig.ARGON_HASH_LENGTH;
        public PasswordManager() { }

        public async Task<string> Hash(string password)
        {   
            return await PasswordHelper.CreateHash(password, DegreesOfParallelism, Iterations, MemorySize, HashLength);
        }

        public Task<bool> IsMatch(string password, string hash)
        {
            return PasswordHelper.VerifyHashAsync(password, hash);
        }
    }
}
