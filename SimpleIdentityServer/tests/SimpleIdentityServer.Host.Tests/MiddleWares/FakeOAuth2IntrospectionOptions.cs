using Microsoft.AspNetCore.Authentication;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public class FakeOAuth2IntrospectionOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "OAuth2Introspection";

        public string WellKnownConfigurationUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
