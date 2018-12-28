namespace SimpleAuth.Uma.Parameters
{
    using System.Collections.Generic;

    public class AddPolicyRuleParameter
    {
        public List<string> Scopes { get; set; }
        public List<string> ClientIdsAllowed { get; set; }
        public List<AddClaimParameter> Claims { get; set; }
        public bool IsResourceOwnerConsentNeeded { get; set; }
        public string Script { get; set; }
        public string OpenIdProvider { get; set; }
    }
}