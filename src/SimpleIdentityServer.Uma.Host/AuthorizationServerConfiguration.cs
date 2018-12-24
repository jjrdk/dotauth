namespace SimpleIdentityServer.Uma.Host
{
    using System;
    using Core.Models;
    using System.Collections.Generic;
    using SimpleAuth.Shared.Models;

    public class AuthorizationServerConfiguration
    {
        public AuthorizationServerConfiguration(
            IEnumerable<ResourceSet> resources = null,
            IEnumerable<Policy> policies = null,
            IEnumerable<Client> clients = null,
            IEnumerable<Scope> scopes = null)
        {
            Resources = new List<ResourceSet>(resources ?? Array.Empty<ResourceSet>());
            Policies = new List<Policy>(policies ?? Array.Empty<Policy>());
            Clients = new List<Client>(clients ?? Array.Empty<Client>());
            Scopes = new List<Scope>(scopes ?? Array.Empty<Scope>());
        }

        public IReadOnlyCollection<ResourceSet> Resources { get; }
        public IReadOnlyCollection<Policy> Policies { get; }
        public IReadOnlyCollection<Client> Clients { get; }
        public IReadOnlyCollection<Scope> Scopes { get; }
    }
}