using System;

namespace BackendServices.JWT
{
    public record JwtToken
    {
        public string Token { get; init; }
        public long ExpiresAt { get; init; }

        public JwtToken(string token, long expiresAt)
        {
            Token = token;
            ExpiresAt = expiresAt;
        }
        
        public JwtToken(string token, DateTime expiredAt) 
            : this(token, (long)(expiredAt - DateTime.UnixEpoch).TotalSeconds)
        {}
    }
}
