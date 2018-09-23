using SimpleIdentityServer.Module;

namespace SimpleIdentityServer.EF.InMemory
{
    public class InMemoryOAuthRepositoryModule : IModule
    {
        public void Init()
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddOAuthInMemoryEF();
        }
    }
}
