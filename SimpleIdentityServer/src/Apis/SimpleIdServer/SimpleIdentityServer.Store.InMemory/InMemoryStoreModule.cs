using SimpleIdentityServer.Module;
using System;

namespace SimpleIdentityServer.Store.InMemory
{
    public class InMemoryStoreModule : IModule
    {
        public void Init()
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleInitialized;
        }

        private void HandleInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddInMemoryStorage();
        }
    }
}