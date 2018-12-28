namespace SimpleAuth.Server.Tests.MiddleWares
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Authentication;

    public class FakeOAuth2IntrospectionOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "OAuth2Introspection";

        public string WellKnownConfigurationUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public HttpClient Client { get; set; }
    }
}
