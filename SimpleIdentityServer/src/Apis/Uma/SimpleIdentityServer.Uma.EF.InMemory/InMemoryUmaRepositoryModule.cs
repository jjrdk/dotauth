using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.EF.InMemory
{
    public class InMemoryUmaRepositoryModule : IModule
    {
        public void Init(IDictionary<string, string> properties)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddUmaInMemoryEF();
        }
    }
}
