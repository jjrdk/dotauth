using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Shell
{
    public class ShellModule : IModule
    {
        public void Init()
        {
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.Initialized += HandleApplicationBuilderInitialized;
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteConfigured += HandleRouteConfigured;
        }

        private void HandleApplicationBuilderInitialized(object sender, EventArgs e)
        {
            var applicationBuilderContext = AspPipelineContext.Instance().ApplicationBuilderContext;
            applicationBuilderContext.App.UseShellStaticFiles();
        }

        private void HandleMvcAdded(object sender, EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddBasicShell(configureServiceContext.MvcBuilder);
        }

        private void HandleRouteConfigured(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteBuilder.UseShell();
        }

        public void Configure(IApplicationBuilder applicationBuilder)
        {
            // applicationBuilder.UseShellStaticFiles();
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
           // routeBuilder.UseShell();
        }

        public void ConfigureAuthentication(AuthenticationBuilder authBuilder, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureAuthorization(AuthorizationOptions authorizationOptions, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null, IEnumerable<ModuleUIDescriptor> moduleUiDescriptors = null)
        {
            /*
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if(mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            if (moduleUiDescriptors == null)
            {
                throw new ArgumentNullException(nameof(moduleUiDescriptors));
            }

            services.AddBasicShell(mvcBuilder, env);
            */
        }

        public ModuleUIDescriptor GetModuleUI()
        {
            return null;
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new string[0];
        }
    }
}
