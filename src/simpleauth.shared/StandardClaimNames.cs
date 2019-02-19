namespace SimpleAuth.Shared
{
    internal static class StandardClaimNames
    {
        public const string Issuer = "iss";
        public const string Audiences = "aud";
        public const string ExpirationTime = "exp";
        public const string Iat = "iat";
        public const string AuthenticationTime = "auth_time";
        public const string Nonce = "nonce";
        public const string Acr = "acr";
        public const string Amr = "amr";
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
        public const string ClientId = "client_id";
        public const string Scopes = "scope";
        public const string Subject = "sub";
    }
}