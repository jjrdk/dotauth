namespace SimpleIdentityServer.Uma.Host
{
    using System.Collections.Generic;
    using Core.Models;

    public class AuthorizationServerConfiguration
    {
        public List<Core.Models.ResourceSet> Resources { get; set; }
        public List<Policy> Policies { get; set; }
        public List<SimpleIdentityServer.Core.Common.Models.Client> Clients { get; set; }
        public List<SimpleIdentityServer.Core.Common.Models.Scope> Scopes { get; set; }
    }
}