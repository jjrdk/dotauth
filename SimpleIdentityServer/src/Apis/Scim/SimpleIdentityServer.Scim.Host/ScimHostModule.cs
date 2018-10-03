using SimpleIdentityServer.Module;
using SimpleIdentityServer.Scim.Host.Extensions;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Host
{
    public class ScimHostModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddScimHost(new ScimServerOptions());
        }
    }
}