namespace SimpleIdentityServer.Shared.Parameters
{
    using System.Collections.Generic;

    public class SearchProfileParameter
    {
        public SearchProfileParameter()
        {
            ResourceOwnerIds = new List<string>();
            Issuers = new List<string>();
        }

        public IEnumerable<string> ResourceOwnerIds { get; set; }
        public IEnumerable<string> Issuers { get; set; }
    }
}
