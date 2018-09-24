using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.AccessToken.Store.InMemory
{
    public class AccessTokenInMemoryModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleInitialized;
        }

        private void HandleInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddInMemoryAccessTokenStore();
        }
    }
}
