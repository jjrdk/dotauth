using System;
using System.Collections.Generic;
using SimpleIdentityServer.Module;

namespace SimpleBus.InMemory
{
    public class SimpleBusInMemoryModule : IModule
    {
        public void Init(IDictionary<string, string> options)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleConfigureServiceContextInitialized;
        }

        private void HandleConfigureServiceContextInitialized(object sender, EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddSimpleBusInMemory(new InMemoryOptions());
        }
    }
}
