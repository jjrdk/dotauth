namespace SimpleAuth.Shared
{
    using System.Collections.Generic;

    public class AccountFilterRuleResult
    {
        public AccountFilterRuleResult(string ruleName)
        {
            RuleName = ruleName;
            ErrorMessages = new List<string>();
        }

        public string RuleName { get; }
        public List<string> ErrorMessages { get; }
        public bool IsValid { get; set; }
    }
}