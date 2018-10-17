namespace SimpleIdentityServer.Core.Common
{
    /// <summary>
    /// Identifies the operation(s) that the key is itended to be user for
    /// </summary>
    public enum KeyOperations
    {
        /// <summary>
        /// Compute digital signature or MAC
        /// </summary>
        Sign = 0,
        /// <summary>
        /// Verify digital signature or MAC
        /// </summary>
        Verify = 1,
        /// <summary>
        /// Encrypt content
        /// </summary>
        Encrypt = 2,
        /// <summary>
        /// Decrypt content and validate decryption if applicable
        /// </summary>
        Decrypt = 3,
        /// <summary>
        /// Encrypt key
        /// </summary>
        WrapKey = 4,
        /// <summary>
        /// Decrypt key and validate encryption if applicable
        /// </summary>
        UnWrapKey = 5,
        /// <summary>
        /// Derive key
        /// </summary>
        DeriveKey = 6,
        /// <summary>
        /// Derive bits not to be used as a key
        /// </summary>
        DeriveBits = 7
    }
}