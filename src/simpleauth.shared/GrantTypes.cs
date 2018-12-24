namespace SimpleIdentityServer.Shared
{
    public static class GrantTypes
    {
        public const string ClientCredentials = "client_credentials";
        public const string Password = "password";
        public const string RefreshToken = "refresh_token";
        public const string AuthorizationCode = "authorization_code";
        public const string ValidateBearer = "validate_bearer";
        public const string UmaTicket = "uma_ticket";
    }
}