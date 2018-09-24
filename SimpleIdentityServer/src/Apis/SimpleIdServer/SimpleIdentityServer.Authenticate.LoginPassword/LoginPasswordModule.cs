using SimpleIdentityServer.Authenticate.Basic;
using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.Authenticate.LoginPassword
{
    public class LoginPasswordModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.MvcAdded += HandleMvcAdded;
            AspPipelineContext.Instance().ApplicationBuilderContext.RouteConfigured += HandleRouteConfigured;
        }

        private void HandleMvcAdded(object sender, System.EventArgs e)
        {
            var configureServiceContext = AspPipelineContext.Instance().ConfigureServiceContext;
            configureServiceContext.Services.AddLoginPasswordAuthentication(configureServiceContext.MvcBuilder, GetOptions());
        }

        private void HandleRouteConfigured(object sender, System.EventArgs e)
        {
            var applicationBuilderContext = AspPipelineContext.Instance().ApplicationBuilderContext;
            applicationBuilderContext.RouteBuilder.UseLoginPasswordAuthentication();
        }

        private BasicAuthenticateOptions GetOptions()
        {
            var result = new BasicAuthenticateOptions();
            if (_properties != null)
            {
                var scimBaseUrl = string.Empty;
                var clientId = string.Empty;
                var clientSecret = string.Empty;
                var authorizationWellKnownConfiguration = string.Empty;
                bool isScimResourceAutomaticallyCreated = false;
                if (_properties.TryGetValue("ScimBaseUrl", out scimBaseUrl))
                {
                    result.ScimBaseUrl = scimBaseUrl;
                }

                if (_properties.TryGetValue("IsScimResourceAutomaticallyCreated", out isScimResourceAutomaticallyCreated))
                {
                    result.IsScimResourceAutomaticallyCreated = isScimResourceAutomaticallyCreated;
                }

                if (_properties.TryGetValue("ClientId", out clientId))
                {
                    result.AuthenticationOptions.ClientId = clientId;
                }

                if (_properties.TryGetValue("ClientSecret", out clientSecret))
                {
                    result.AuthenticationOptions.ClientSecret = clientSecret;
                }

                if (_properties.TryGetValue("AuthorizationWellKnownConfiguration", out authorizationWellKnownConfiguration))
                {
                    result.AuthenticationOptions.AuthorizationWellKnownConfiguration = authorizationWellKnownConfiguration;
                }

                result.ClaimsIncludedInUserCreation = _properties.TryGetArr("ClaimsIncludedInUserCreation");                
            }

            return result;
        }
    }
}
