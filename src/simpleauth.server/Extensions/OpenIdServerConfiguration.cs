namespace SimpleAuth.Server.Extensions
{
    using Shared.Models;
    using System.Collections.Generic;

    public class OpenIdServerConfiguration
    {
        public IReadOnlyCollection<ResourceOwner> Users { get; set; }
        public IReadOnlyCollection<Client> Clients { get; set; }
        public IReadOnlyCollection<Translation> Translations { get; set; }
        //public IReadOnlyCollection<JsonWebKey> JsonWebKeys { get; set; }
        public IReadOnlyCollection<Consent> Consents { get; set; }
        public IReadOnlyCollection<ResourceOwnerProfile> Profiles { get; set; }
    }
}