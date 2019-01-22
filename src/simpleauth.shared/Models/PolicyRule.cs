namespace SimpleAuth.Shared.Models
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public class PolicyRule
    {
        public string Id { get; set; }
        public List<string> ClientIdsAllowed { get; set; }
        public List<string> Scopes { get; set; }
        public List<Claim> Claims { get; set; }
        public bool IsResourceOwnerConsentNeeded { get; set; }
        public string Script { get; set; }
        public string OpenIdProvider { get; set; }
    }
}