namespace SimpleIdentityServer.Core.WebSite.Consent.Actions
{
    using System.Collections.Generic;
    using Results;
    using SimpleAuth.Shared.Models;

    public class DisplayContentResult
    {
        public Client Client { get; set; }
        public ICollection<Scope> Scopes { get; set; }
        public ICollection<string> AllowedClaims { get; set; }
        public EndpointResult EndpointResult { get; set; }
    }
}