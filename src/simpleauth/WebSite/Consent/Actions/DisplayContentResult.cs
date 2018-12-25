namespace SimpleAuth.WebSite.Consent.Actions
{
    using System.Collections.Generic;
    using Results;
    using Shared.Models;

    public class DisplayContentResult
    {
        public Client Client { get; set; }
        public ICollection<Scope> Scopes { get; set; }
        public ICollection<string> AllowedClaims { get; set; }
        public EndpointResult EndpointResult { get; set; }
    }
}