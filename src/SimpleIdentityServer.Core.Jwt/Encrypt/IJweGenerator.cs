namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    using Common;

    public interface IJweGenerator
    {
        string GenerateJwe(
            string entry,
            JweAlg alg,
            JweEnc enc,
            JsonWebKey jsonWebKey);

        string GenerateJweByUsingSymmetricPassword(
            string entry,
            JweAlg alg,
            JweEnc enc,
            JsonWebKey jsonWebKey,
            string password);
    }
}