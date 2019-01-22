namespace SimpleAuth.Parameters
{
    using System.Collections.Generic;

    public class UpdatePolicyRuleParameter
    {
        public string Id { get; set; }
        public List<string> ClientIdsAllowed { get; set; }
        public List<string> Scopes { get; set; }
        public string Script { get; set; }
        public bool IsResourceOwnerConsentNeeded { get; set; }
        public List<AddClaimParameter> Claims { get; set; }
        public string OpenIdProvider { get; set; }
    }
}