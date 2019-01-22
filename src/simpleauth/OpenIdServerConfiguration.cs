namespace SimpleAuth
{
    using System.Collections.Generic;
    using Shared.Models;

    public class OpenIdServerConfiguration
    {
        public IReadOnlyCollection<ResourceOwner> Users { get; set; }
        public IReadOnlyCollection<Client> Clients { get; set; }
        public IReadOnlyCollection<Shared.Models.Translation> Translations { get; set; }
        //public IReadOnlyCollection<JsonWebKey> JsonWebKeys { get; set; }
        public IReadOnlyCollection<Consent> Consents { get; set; }
        public IReadOnlyCollection<ResourceOwnerProfile> Profiles { get; set; }
        public IReadOnlyCollection<Scope> Scopes { get; set; }
    }
}