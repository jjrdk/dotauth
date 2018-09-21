using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdentityServer.Module
{
    public class AspPipelineContext
    {
        private ConfigureServiceContext _configureServiceContext;
        private static AspPipelineContext _instance;

        private AspPipelineContext() { }

        public static AspPipelineContext Instance()
        {
            if (_instance == null)
            {
                _instance = new AspPipelineContext();
            }

            return _instance;
        }

        public void StartConfigureServices(IServiceCollection services)
        {
            _configureServiceContext = new ConfigureServiceContext(services);
        }

        public ConfigureServiceContext ConfigureServiceContext
        {
            get
            {
                return _configureServiceContext;
            }
        }
    }
}
