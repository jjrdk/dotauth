namespace SimpleAuth.Shared.DTOs
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class PostPolicyRule
    {
        [DataMember(Name = PolicyRuleNames.ClientIdsAllowed)]
        public List<string> ClientIdsAllowed { get; set; }
        [DataMember(Name = PolicyRuleNames.Scopes)]
        public List<string> Scopes { get; set; }
        [DataMember(Name = PolicyRuleNames.Claims)]
        public List<PostClaim> Claims { get; set; }
        [DataMember(Name = PolicyRuleNames.IsResourceOwnerConsentNeeded)]
        public bool IsResourceOwnerConsentNeeded { get; set; }
        [DataMember(Name = PolicyRuleNames.Script)]
        public string Script { get; set; }
        [DataMember(Name = PolicyRuleNames.OpenIdProvider)]
        public string OpenIdProvider { get; set; }
    }
}