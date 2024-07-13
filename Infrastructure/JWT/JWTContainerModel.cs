using BackendServices.JWT;
using System.Collections.Generic;
using System.Security.Claims;

namespace Infrastructure.JWT
{
    internal record JWTContainerModel : IAuthContainerModel
    {
        string IAuthContainerModel.SecretKey => SecretKey;
        internal string SecretKey { get; init; }
        public string SecurityAlgorithm { get; init; }
        public long ExpiresIn { get; init; }
#nullable enable
        public IEnumerable<Claim>? Claims { get; init; }

        public JWTContainerModel() { }

        public IAuthContainerModel WithSecretKey(string secretKey)
        {
            return this with { SecretKey = secretKey };
        }

        public IAuthContainerModel WithAlgorithm(string securityAlgorithm)
        {
            return this with { SecurityAlgorithm = securityAlgorithm };
        }

        public IAuthContainerModel WithExpiresMinutes(long expires)
        {
            return this with { ExpiresIn = expires };
        }

        public IAuthContainerModel WithClaimes(IEnumerable<Claim>? claimes)
        {
            return this with { Claims = claimes };
        }

#nullable restore
    }
}
