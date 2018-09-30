using System.Collections.Generic;
using SimpleIdentityServer.Module;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.InMemory
{
    public class AccountFilterInMemoryModule : IModule
    {
        public void Init(IDictionary<string, string> options)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddBasicAccountFilterInMemoryEF();
        }
    }
}
