using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleIdentityServer.Module
{
    public class AspPipelineContext
    {
        private readonly ConfigureServiceContext _configureServiceContext;
        private readonly ApplicationBuilderContext _applicationBuilderContext;
        private static AspPipelineContext _instance;

        private AspPipelineContext()
        {
            _configureServiceContext = new ConfigureServiceContext();
            _applicationBuilderContext = new ApplicationBuilderContext();
        }

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
            _configureServiceContext.Init(services);
        }

        public void StartConfigureApplicationBuilder(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _applicationBuilderContext.Init(app, env, loggerFactory);
        }

        public ConfigureServiceContext ConfigureServiceContext
        {
            get
            {
                return _configureServiceContext;
            }
        }

        public ApplicationBuilderContext ApplicationBuilderContext
        {
            get
            {
                return _applicationBuilderContext;
            }
        }
    }
}
