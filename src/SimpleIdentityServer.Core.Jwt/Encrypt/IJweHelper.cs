namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    using Encryption;
    using Shared;

    public interface IJweHelper
    {
        IEncryption GetEncryptor(JweEnc enc);
    }
}