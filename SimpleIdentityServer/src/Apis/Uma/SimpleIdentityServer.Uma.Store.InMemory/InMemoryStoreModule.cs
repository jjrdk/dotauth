using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Store.InMemory
{
    public class InMemoryStoreModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddUmaInMemoryStore();
        }
    }
}
