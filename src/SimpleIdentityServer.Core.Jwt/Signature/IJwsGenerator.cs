namespace SimpleIdentityServer.Core.Jwt.Signature
{
    using Common;

    public interface IJwsGenerator
    {
        string Generate(
            JwsPayload payload,
            JwsAlg jwsAlg,
            JsonWebKey jsonWebKey);
    }
}