namespace SimpleAuth.OAuth2Introspection
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Authentication;

    public class OAuth2IntrospectionOptions : AuthenticationSchemeOptions
   {
        public const string AuthenticationScheme = "OAuth2Introspection";

        public string WellKnownConfigurationUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public HttpClientHandler BackChannelHttpHandler { get; set; }
    }
}
