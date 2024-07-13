using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;

[assembly:InternalsVisibleTo("Infrastructure")]
[assembly: InternalsVisibleTo("Infrastructure.Unit.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace BackendServices.JWT
{
    public interface IAuthContainerModel
    {
        #region Members

        internal string SecretKey { get;}
        string SecurityAlgorithm { get; }
        long ExpiresIn { get;}
#nullable enable
        IEnumerable<Claim>? Claims { get; }

        #endregion

        IAuthContainerModel WithSecretKey(string secretKey);
        IAuthContainerModel WithAlgorithm(string securityAlgorithm);
        IAuthContainerModel WithExpiresMinutes(long expires);
        IAuthContainerModel WithClaimes(IEnumerable<Claim>? claimes);

#nullable disable
    }
}
