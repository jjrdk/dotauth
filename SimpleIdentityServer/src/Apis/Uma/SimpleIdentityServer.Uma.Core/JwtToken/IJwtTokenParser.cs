namespace SimpleIdentityServer.Uma.Core.JwtToken
{
    using SimpleIdentityServer.Core.Common;
    using SimpleIdentityServer.Core.Common.DTOs.Requests;

    public interface IJwtTokenParser
    {
        JwsPayload UnSign(string jws, string openidUrl, JsonWebKeySet jsonWebKeySet);
    }
}