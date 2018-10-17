namespace SimpleIdentityServer.Authenticate.Basic
{
    public class BasicAuthenticationOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationWellKnownConfiguration { get; set; }
    }
}