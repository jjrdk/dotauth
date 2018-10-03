using SimpleIdentityServer.Module;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Host
{
    public class UmaHostModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            var services = AspPipelineContext.Instance().ConfigureServiceContext.Services;
            services.AddUmaHost(new AuthorizationServerOptions());
        }
    }
}