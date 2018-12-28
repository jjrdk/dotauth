namespace SimpleAuth.Uma
{
    using SimpleAuth;

    public class AuthorizationServerOptions
    {
        public UmaConfigurationOptions UmaConfigurationOptions { get; set; }
        public OAuthConfigurationOptions OAuthConfigurationOptions { get; set; }
        public AuthorizationServerConfiguration Configuration { get; set; }
    }
}
