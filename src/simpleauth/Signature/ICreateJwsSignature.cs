namespace SimpleAuth.Signature
{
    using Shared;

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