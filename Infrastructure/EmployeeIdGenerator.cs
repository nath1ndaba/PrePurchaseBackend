using BackendServices;
using System;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class EmployeeIdGenerator : IEmployeeIdGenerator
    {
        public string GetNew()
            => $"{DateTime.UtcNow:yyMMdd}"+Nanoid.Nanoid.Generate(PrePurchaseConfig.NANOID_ALPHABET, PrePurchaseConfig.NANOID_ID_LENGTH);

        public async Task<string> GetNewAsync()
            => $"{DateTime.UtcNow:yyMMdd}"+ await Nanoid.Nanoid.GenerateAsync(PrePurchaseConfig.NANOID_ALPHABET, PrePurchaseConfig.NANOID_ID_LENGTH);
    }
}
