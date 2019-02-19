namespace SimpleAuth.Shared.Models
{
    /// <summary>
    /// Defines the client secret types.
    /// </summary>
    public enum ClientSecretTypes
    {
        /// <summary>
        /// Shared secret
        /// </summary>
        SharedSecret = 0,

        /// <summary>
        /// X509 thumbprint
        /// </summary>
        X509Thumbprint = 1,

        /// <summary>
        /// X509 name
        /// </summary>
        X509Name = 2
    }
}