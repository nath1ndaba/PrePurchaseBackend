using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace BackendServices.JWT
{
    internal class JwtAuthOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IAuthService _authService;
        public JwtAuthOptions(IAuthService authService)
        {
            _authService = authService;
        }
        public void Configure(JwtBearerOptions options)
        {
            Configure(string.Empty, options);
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            if (name != JwtBearerDefaults.AuthenticationScheme)
                return;

            options.TokenValidationParameters = _authService.GetTokenValidationParameters();

        }
    }
}
