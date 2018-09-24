using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.EF.InMemory
{
    public class InMemoryOAuthRepositoryModule : IModule
    {
        public void Init(IDictionary<string, string> options)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddOAuthInMemoryEF();
        }
    }
}
