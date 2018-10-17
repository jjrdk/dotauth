namespace SimpleIdentityServer.Core.Jwt.Signature
{
    using Common;
    using Common.DTOs.Requests;

    public interface IJwsParser
    {
        JwsPayload ValidateSignature(string jws, JsonWebKey jsonWebKey);
        JwsPayload ValidateSignature(string jws, JsonWebKeySet jsonWebKeySet);
        JwsProtectedHeader GetHeader(string jws);
        JwsPayload GetPayload(string jws);
    }
}