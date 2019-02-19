namespace SimpleAuth.Shared.Models
{
    /// <summary>
    /// Defines the token endpoint authentication methods.
    /// </summary>
    public static class TokenEndPointAuthenticationMethods
    {
        /// <summary>
        /// Client secret basic
        /// </summary>
        public const string ClientSecretBasic = "client_secret_basic";

        /// <summary>
        /// Client secret post
        /// </summary>
        public const string ClientSecretPost = "client_secret_post";

        /// <summary>
        /// Client secret JWT
        /// </summary>
        public const string ClientSecretJwt = "client_secret_jwt";

        /// <summary>
        /// Private key JWT
        /// </summary>
        public const string PrivateKeyJwt = "private_key_jwt";

        /// <summary>
        /// TLS client authentication
        /// </summary>
        public const string TlsClientAuth = "tls_client_auth";

        /// <summary>
        /// None
        /// </summary>
        public const string None = "none";
    }
}