namespace SimpleAuth.Shared
{
    /// <summary>
    /// Key types
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// Ellipse Curve
        /// </summary>
        EC = 0,
        /// <summary>
        /// RSA
        /// </summary>
        RSA = 1,
        /// <summary>
        /// Octet sequence (used to represent symmetric keys)
        /// </summary>
        oct = 2
    }
}