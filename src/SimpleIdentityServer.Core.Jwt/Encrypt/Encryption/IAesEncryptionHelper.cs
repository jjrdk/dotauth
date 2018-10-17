namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    using Common;

    public interface IAesEncryptionHelper
    {
        byte[] GenerateContentEncryptionKey(int keySize);

        byte[] EncryptContentEncryptionKey(
            byte[] contentEncryptionKey,
            JweAlg alg,
            JsonWebKey jsonWebKey);

        byte[] DecryptContentEncryptionKey(
            byte[] encryptedContentEncryptionKey,
            JweAlg alg,
            JsonWebKey jsonWebKey);

        byte[] EncryptWithAesAlgorithm(
            string toEncrypt,
            byte[] key,
            byte[] iv);

        string DecryptWithAesAlgorithm(
            byte[] cipherText,
            byte[] key,
            byte[] iv);
    }
}