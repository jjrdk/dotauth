namespace DotAuth.Shared.Models;

using System;

/// <summary>
/// Defines the account filtering result.
/// </summary>
public sealed record AccountFilterResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountFilterResult"/> class.
    /// </summary>
    public AccountFilterResult(bool isValid)
        : this(isValid, Array.Empty<AccountFilterRuleResult>()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountFilterResult"/> class.
    /// </summary>
    public AccountFilterResult(bool isValid, AccountFilterRuleResult[] accountFilterRules)
    {
        IsValid = isValid;
        AccountFilterRules = accountFilterRules;
    }

    /// <summary>
    /// Returns true if account is valid.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
    /// </value>
    public bool IsValid { get; }

    /// <summary>
    /// Gets or sets the account filter rules.
    /// </summary>
    /// <value>
    /// The account filter rules.
    /// </value>
    public AccountFilterRuleResult[] AccountFilterRules { get; }
}