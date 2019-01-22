namespace SimpleAuth.Shared
{
    using System.Collections.Generic;

    public class AccountFilterResult
    {
        public AccountFilterResult()
        {
            AccountFilterRules = new List<AccountFilterRuleResult>();
        }

        public bool IsValid { get; set; }
        public IEnumerable<AccountFilterRuleResult> AccountFilterRules { get; set; }
    }
}
