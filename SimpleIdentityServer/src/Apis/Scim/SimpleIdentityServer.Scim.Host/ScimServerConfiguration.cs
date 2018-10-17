namespace SimpleIdentityServer.Scim.Host
{
    using System.Collections.Generic;
    using Core.EF.Models;

    public class ScimServerConfiguration
    {
        public List<Representation> Representations { get; set; }
        public List<Schema> Schemas { get; set; }
    }
}