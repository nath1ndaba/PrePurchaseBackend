using BackendServices.JWT;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.JWT
{
    internal class JWTAuthService : IAuthService
    {

        #region Members

        /// <summary>
        /// The secret key we use to encrypt out token with.
        /// </summary>
        internal string SecretKey { get; }
        internal string SecurityAlgorithm { get; }
        internal long ExpiresIn { get; }
        internal string? Issuer { get; }
        internal string? Audience { get; }
        internal bool ValidateIssuer { get; }
        internal bool ValidateAudience { get; }

        #endregion

        #region Constructor

        public JWTAuthService(
            string secretKey
            , string securityAlgorithm = SecurityAlgorithms.HmacSha512Signature,
            long expiresIn = 600, string? issuer = null, string? audience = null
            , bool validateIssuer = false, bool validateAudience = false)
        {
            SecretKey = secretKey;
            SecurityAlgorithm = securityAlgorithm;
            ExpiresIn = expiresIn;
            Issuer = issuer;
            Audience = audience;
            ValidateIssuer = validateIssuer;
            ValidateAudience = validateAudience;
        }

        #endregion

        /// <summary>
        /// Generates token by given model.
        /// Validates whether the given token is valid, then gets the symmetric key.
        /// Encrypt the token and returns it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Generated string token</returns>
        public string GenerateToken(IAuthContainerModel model)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            SecurityToken securityToken = GenerateSecurityToken(model, jwtSecurityTokenHandler);
            string token = jwtSecurityTokenHandler.WriteToken(securityToken);

            return token;
        }

        /// <summary>
        /// Generates token by given model.
        /// Validates whether the given token is valid, then gets the symmetric key.
        /// Encrypt the token and returns it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Generated SecurityToken token</returns>
        JwtToken IAuthService.GenerateJwtToken(IAuthContainerModel model)
            => GenerateSecurityToken(model).ToJwtToken();

        /// <summary>
        /// <returns>IEnumerable of claims for the given token.</returns> 
        /// Receives the claims of the token by given token string.
        /// </summary>
        /// <remarks>
        /// Pay attention, one the token is FAKE will throw exception.
        /// </remarks>
        /// <param name="token"></param>
        /// <returns>IEnumerable of claims for the given token.</returns>

        public IEnumerable<Claim> GetTokenClaims(string token, string? secretKey = default)
        {
            return GetClaimsPrincipal(token, secretKey!).Claims;
        }

        /// <summary>
        /// <returns>ClaimsPrincipal for the given token.</returns>
        /// Receives the claims of the token by given token string.
        /// </summary>
        /// <remarks>
        /// Pay attention! When the token is FAKE will throw exception.
        /// </remarks>
        /// <param name="token"></param>

        public ClaimsPrincipal GetClaimsPrincipal(string token, string? secretKey = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Given token is null or empty");

            TokenValidationParameters tokenValidationParameters = GetTokenValidationParameters(secretKey!);
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            ClaimsPrincipal tokenValid = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return tokenValid;
        }

        /// <summary>
        /// Validates whether a given token is valid or not, and returns true in case the is valid otherwise it will return false
        /// </summary>
        /// <param name="token"></param>
        /// <returns>true or false</returns>

        public bool IsTokenValid(string token)
        {
            try
            {
                ClaimsPrincipal tokenValid = GetClaimsPrincipal(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        IEnumerable<Claim>? IAuthService.PeekClaims(string token)
            => PeekClaims(token);

        #region Private Methods

        private SecurityKey GetSymmetricSecurityKey(string secretKey)
        {
            byte[] symmetricKey;
            secretKey ??= SecretKey;
            try
            {
                symmetricKey = Convert.FromBase64String(secretKey);
            }
            catch
            {
                string? base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(secretKey));
                symmetricKey = Convert.FromBase64String(base64Key);
            }
            return new SymmetricSecurityKey(symmetricKey);
        }

        private SecurityToken GenerateSecurityToken(IAuthContainerModel model, JwtSecurityTokenHandler? jwtSecurityTokenHandler = default)
        {
            if (model == null)
                throw new ArgumentException($"Arguments to create token are not valid. {nameof(IAuthContainerModel)} is null!");

            string? _securityAlgorithm = model.SecurityAlgorithm ?? SecurityAlgorithm;
            SecurityTokenDescriptor securityTokenDescriptor = new()
            {
                Subject = new(model.Claims),
                SigningCredentials = new(GetSymmetricSecurityKey(model.SecretKey),
                _securityAlgorithm),
                Issuer = Issuer,
                Audience = Audience
            };

            long expiresIn = model.ExpiresIn > 0 ? model.ExpiresIn : ExpiresIn;
            // a number less than zero means it never expires
            if (expiresIn > 0)
            {
                securityTokenDescriptor.Expires = DateTime.UtcNow.AddSeconds(expiresIn);
            }

            jwtSecurityTokenHandler ??= new();
            SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

            return securityToken;

        }

        internal TokenValidationParameters GetTokenValidationParameters(string? secretKey = default)
        {
            return new()
            {
                ValidateIssuer = ValidateIssuer,
                ValidateAudience = ValidateAudience,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = GetSymmetricSecurityKey(secretKey!),
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        TokenValidationParameters IAuthService.GetTokenValidationParameters(string secretKey)
            => GetTokenValidationParameters(secretKey);

        #endregion

        #region Static Methods

#nullable enable
        public static IEnumerable<Claim>? PeekClaims(string token)
#nullable restore
        {
            JwtSecurityTokenHandler tokenHandler = new();

            try
            {
                return tokenHandler.ReadJwtToken(token).Claims;
            }
            catch
            {
                return default;
            }
        }

        public static string SecurityTokenToString(SecurityToken securityToken)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            return jwtSecurityTokenHandler.WriteToken(securityToken);
        }

        #endregion

    }
}
