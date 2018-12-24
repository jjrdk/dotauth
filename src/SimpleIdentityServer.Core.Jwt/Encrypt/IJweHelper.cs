namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    using Encryption;
    using SimpleAuth.Shared;

    public interface IJweHelper
    {
        IEncryption GetEncryptor(JweEnc enc);
    }
}