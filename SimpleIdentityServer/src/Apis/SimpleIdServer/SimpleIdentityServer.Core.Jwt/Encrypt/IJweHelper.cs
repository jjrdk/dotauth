namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    using Common;
    using Encryption;

    public interface IJweHelper
    {
        IEncryption GetEncryptor(JweEnc enc);
    }
}