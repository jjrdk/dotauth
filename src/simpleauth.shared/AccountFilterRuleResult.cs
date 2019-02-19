namespace SimpleAuth.Shared
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the result of the account filter rule.
    /// </summary>
    public class AccountFilterRuleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountFilterRuleResult"/> class.
        /// </summary>
        /// <param name="ruleName">Name of the rule.</param>
        public AccountFilterRuleResult(string ruleName)
        {
            RuleName = ruleName;
            ErrorMessages = new List<string>();
        }

        /// <summary>
        /// Gets the name of the rule.
        /// </summary>
        /// <value>
        /// The name of the rule.
        /// </value>
        public string RuleName { get; }

        /// <summary>
        /// Gets the error messages.
        /// </summary>
        /// <value>
        /// The error messages.
        /// </value>
        public List<string> ErrorMessages { get; }

        /// <summary>
        /// Returns true if the rule is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; set; }
    }
}