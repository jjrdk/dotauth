namespace SimpleAuth.Uma.JwtToken
{
    using Shared;
    using Shared.Requests;

    public interface IJwtTokenParser
    {
        JwsPayload UnSign(string jws, string openidUrl, JsonWebKeySet jsonWebKeySet);
    }
}