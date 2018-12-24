namespace SimpleAuth.Shared
{
    public static class StandardClaimNames
    {
        public static string Issuer = "iss";
        public static string Audiences = "aud";
        public static string ExpirationTime = "exp";
        public static string Iat = "iat";
        public static string AuthenticationTime = "auth_time";
        public static string Nonce = "nonce";
        public static string Acr = "acr";
        public static string Amr = "amr";
        public static string Azp = "azp";
        /// <summary>
        /// Unique identifier of the JWT.
        /// </summary>
        public static string Jti = "jti";
        /// <summary>
        /// Access token hash value
        /// </summary>
        public static string AtHash = "at_hash";
        /// <summary>
        /// Authorization code hash value
        /// </summary>
        public static string CHash = "c_hash";
        public static string ClientId = "client_id";
        public static string Scopes = "scope";
    }
}