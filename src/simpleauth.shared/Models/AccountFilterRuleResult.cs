namespace DotAuth.Shared.Models;

using System.Linq;

/// <summary>
/// Defines the result of the account filter rule.
/// </summary>
public sealed record AccountFilterRuleResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountFilterRuleResult"/> class.
    /// </summary>
    /// <param name="ruleName">Name of the rule.</param>
    /// <param name="isValid">Designates whether the result is valid</param>
    /// <param name="errorMessages">The error messages for the result.</param>
    public AccountFilterRuleResult(string ruleName, bool isValid, params string[] errorMessages)
    {
        RuleName = ruleName;
        IsValid = isValid;
        ErrorMessages = errorMessages.ToArray();
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
    public string[] ErrorMessages { get; }

    /// <summary>
    /// Returns true if the rule is valid.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
    /// </value>
    public bool IsValid { get; }
}