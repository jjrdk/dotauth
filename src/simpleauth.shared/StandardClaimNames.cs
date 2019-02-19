namespace SimpleAuth.Shared
{
    /// <summary>
    /// Parameter names of a token request
    /// </summary>
    internal static class StandardTokenRequestParameterNames
    {
        public const string ClientIdName = "client_id";
        public const string UserName = "username";
        public const string PasswordName = "password";
        public const string AuthorizationCodeName = "code";
        public const string RefreshToken = "refresh_token";
        public const string ScopeName = "scope";
    }

    /// <summary>
    /// Defines the standard claim names.
    /// </summary>
    public static class StandardClaimNames
    {
        /// <summary>
        /// The issuer
        /// </summary>
        public const string Issuer = "iss";

        /// <summary>
        /// The audiences
        /// </summary>
        public const string Audiences = "aud";

        /// <summary>
        /// The expiration time
        /// </summary>
        public const string ExpirationTime = "exp";

        /// <summary>
        /// The iat
        /// </summary>
        public const string Iat = "iat";

        /// <summary>
        /// The authentication time
        /// </summary>
        public const string AuthenticationTime = "auth_time";

        /// <summary>
        /// The nonce
        /// </summary>
        public const string Nonce = "nonce";

        /// <summary>
        /// The acr
        /// </summary>
        public const string Acr = "acr";

        /// <summary>
        /// The amr
        /// </summary>
        public const string Amr = "amr";

        /// <summary>
        /// The azp
        /// </summary>
        public const string Azp = "azp";

        /// <summary>
        /// Unique identifier of the JWT.
        /// </summary>
        public const string Jti = "jti";

        /// <summary>
        /// Access token hash value
        /// </summary>
        public const string AtHash = "at_hash";

        /// <summary>
        /// Authorization code hash value
        /// </summary>
        public const string CHash = "c_hash";

        /// <summary>
        /// The client identifier
        /// </summary>
        public const string ClientId = "client_id";

        /// <summary>
        /// The scopes
        /// </summary>
        public const string Scopes = "scope";

        /// <summary>
        /// The subject
        /// </summary>
        public const string Subject = "sub";
    }
}