namespace SimpleIdentityServer.Uma.Core
{
    using SimpleAuth;

    public class AuthorizationServerOptions
    {
        public Core.UmaConfigurationOptions UmaConfigurationOptions { get; set; }
        public OAuthConfigurationOptions OAuthConfigurationOptions { get; set; }
        public AuthorizationServerConfiguration Configuration { get; set; }
    }
}
