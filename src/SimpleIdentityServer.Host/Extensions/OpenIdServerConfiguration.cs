namespace SimpleIdentityServer.Host.Extensions
{
    using System.Collections.Generic;
    using Shared;
    using Shared.Models;

    public class OpenIdServerConfiguration
    {
        public List<ResourceOwner> Users { get; set; }
        public List<Client> Clients { get; set; }
        public List<Translation> Translations { get; set; }
        public List<JsonWebKey> JsonWebKeys { get; set; }
    }
}