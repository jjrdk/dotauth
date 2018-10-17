namespace SimpleIdentityServer.Uma.Host
{
    using System.Collections.Generic;
    using Core.Models;
    using SimpleIdentityServer.Core.Common.Models;

    public class AuthorizationServerConfiguration
    {
        public List<ResourceSet> Resources { get; set; }
        public List<Policy> Policies { get; set; }
        public List<Client> Clients { get; set; }
        public List<Scope> Scopes { get; set; }
    }
}