namespace SimpleAuth.Shared.Models
{
    /// <summary>
    /// Defines the token endpoint authentication methods.
    /// </summary>
    public enum TokenEndPointAuthenticationMethods
    {
        /// <summary>
        /// Client secret basic
        /// </summary>
        ClientSecretBasic = 0,

        /// <summary>
        /// Client secret post
        /// </summary>
        ClientSecretPost = 1,

        /// <summary>
        /// Client secret JWT
        /// </summary>
        ClientSecretJwt = 2,

        /// <summary>
        /// Private key JWT
        /// </summary>
        PrivateKeyJwt = 3,

        /// <summary>
        /// TLS client authentication
        /// </summary>
        TlsClientAuth = 4,

        /// <summary>
        /// None
        /// </summary>
        None = 5
    }
}