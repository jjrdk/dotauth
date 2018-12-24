namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    using SimpleAuth.Shared;

    public interface IJweParser
    {
        string Parse(
            string jwe,
            JsonWebKey jsonWebKey);

        string ParseByUsingSymmetricPassword(
            string jwe,
            JsonWebKey jsonWebKey,
            string password);

        JweProtectedHeader GetHeader(string jwe);
    }
}