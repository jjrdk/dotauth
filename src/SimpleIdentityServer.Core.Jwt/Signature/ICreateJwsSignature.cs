namespace SimpleIdentityServer.Core.Jwt.Signature
{
    using SimpleAuth.Shared;

    public interface ICreateJwsSignature
    {
        string SignWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string combinedJwsNotSigned);
        bool VerifyWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string input,
            byte[] signature);
    }
}