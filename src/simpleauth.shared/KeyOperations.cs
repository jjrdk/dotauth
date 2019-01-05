namespace SimpleAuth.Shared
{
    /// <summary>
    /// Identifies the operation(s) that the key is intended to be user for.
    /// </summary>
    public static class KeyOperations
    {
        /// <summary>
        /// Compute digital signature or MAC
        /// </summary>
        public static readonly string Sign = "sign";

        /// <summary>
        /// Verify digital signature or MAC
        /// </summary>
        public static readonly string Verify = "verify";

        /// <summary>
        /// Encrypt content
        /// </summary>
        public static readonly string Encrypt = "encrypt";

        /// <summary>
        /// Decrypt content and validate decryption if applicable
        /// </summary>
        public static readonly string Decrypt = "decrypt";

        /// <summary>
        /// Encrypt key
        /// </summary>
        public static readonly string WrapKey = "wrapKey";

        /// <summary>
        /// Decrypt key and validate encryption if applicable
        /// </summary>
        public static readonly string UnWrapKey = "unwrapKey";

        /// <summary>
        /// Derive key
        /// </summary>
        public static readonly string DeriveKey = "deriveKey";

        /// <summary>
        /// Derive bits not to be used as a key
        /// </summary>
        public static readonly string DeriveBits = "deriveBits";
    }
}