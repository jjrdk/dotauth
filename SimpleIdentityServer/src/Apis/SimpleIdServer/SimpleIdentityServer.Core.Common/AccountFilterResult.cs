﻿namespace SimpleIdentityServer.Core.Common
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
