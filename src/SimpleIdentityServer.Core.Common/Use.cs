namespace SimpleIdentityServer.Core.Common
{
    /// <summary>
    /// Identifies the itended use of the Public Key.
    /// </summary>
    public enum Use
    {
        /// <summary>
        /// Signature
        /// </summary>
        Sig = 0,
        /// <summary>
        /// Encryption
        /// </summary>
        Enc = 1
    }
}