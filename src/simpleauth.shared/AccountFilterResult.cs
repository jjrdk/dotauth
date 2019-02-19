namespace SimpleAuth.Shared
{
    using System;

    /// <summary>
    /// Defines the account filtering result.
    /// </summary>
    public class AccountFilterResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountFilterResult"/> class.
        /// </summary>
        public AccountFilterResult()
        {
            AccountFilterRules = Array.Empty<AccountFilterRuleResult>();
        }

        /// <summary>
        /// Returns true if account is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the account filter rules.
        /// </summary>
        /// <value>
        /// The account filter rules.
        /// </value>
        public AccountFilterRuleResult[] AccountFilterRules { get; set; }
    }
}
