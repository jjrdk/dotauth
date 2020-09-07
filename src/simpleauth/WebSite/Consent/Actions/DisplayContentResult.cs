namespace SimpleAuth.WebSite.Consent.Actions
{
    using System.Collections.Generic;
    using Results;
    using Shared.Models;

    internal class DisplayContentResult
    {
        public DisplayContentResult(EndpointResult endpointResult)
        {
            EndpointResult = endpointResult;
        }

        public DisplayContentResult(
            Client client,
            ICollection<Scope> scopes,
            ICollection<string> allowedClaims,
            EndpointResult endpointResult)
        : this(endpointResult)
        {
            Client = client;
            Scopes = scopes;
            AllowedClaims = allowedClaims;
        }

        public Client? Client { get; }

        public ICollection<Scope> Scopes { get; } = new List<Scope>();

        public ICollection<string> AllowedClaims { get; } = new List<string>();

        public EndpointResult EndpointResult { get; }
    }
}
