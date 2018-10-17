using SimpleIdentityServer.Core;

namespace SimpleIdentityServer.Uma.Host
{
    public class AuthorizationServerOptions
    {
        public Core.UmaConfigurationOptions UmaConfigurationOptions { get; set; }
        public OAuthConfigurationOptions OAuthConfigurationOptions { get; set; }
        public AuthorizationServerConfiguration Configuration { get; set; }
    }
}
