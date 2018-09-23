using Microsoft.AspNetCore.Authentication.Cookies;
using SimpleIdentityServer.Module;

namespace SimpleIdentityServer.Host
{
    public class SimpleIdentityServerHostModule : IModule
    {
        public void Init()
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationAdded += HandleAuthorizationAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.Initialized += HandleApplicationBuilderInitialized;
        }

        private void HandleAuthorizationAdded(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.AuthorizationOptions.AddOpenIdSecurityPolicy(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddOpenIdApi(o => { });
        }

        private void HandleApplicationBuilderInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ApplicationBuilderContext.App.UseOpenIdApi(new IdentityServerOptions());
        }
    }
}
