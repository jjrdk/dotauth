namespace SimpleAuth.Uma.JwtToken
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;

    public interface IJwtTokenParser
    {
        JwsPayload UnSign(string jws, string openidUrl, JsonWebKeySet jsonWebKeySet);
    }
}