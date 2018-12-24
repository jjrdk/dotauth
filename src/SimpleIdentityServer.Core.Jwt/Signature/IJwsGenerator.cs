namespace SimpleIdentityServer.Core.Jwt.Signature
{
    using SimpleAuth.Shared;

    public interface IJwsGenerator
    {
        string Generate(
            JwsPayload payload,
            JwsAlg jwsAlg,
            JsonWebKey jsonWebKey);
    }
}