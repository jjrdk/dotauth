namespace DotAuth.Shared.Models;

/// <summary>
/// Defines the filter rule content.
/// </summary>
public sealed record FilterRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterRule"/> class.
    /// </summary>
    public FilterRule(string claimType, string claimValue, ComparisonOperations operation)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
        Operation = operation;
    }

    /// <summary>
    /// Gets or sets the claim key.
    /// </summary>
    /// <value>
    /// The claim key.
    /// </value>
    public string ClaimType { get; }

    /// <summary>
    /// Gets or sets the claim value.
    /// </summary>
    /// <value>
    /// The claim value.
    /// </value>
    public string ClaimValue { get; }

    /// <summary>
    /// Gets or sets the operation.
    /// </summary>
    /// <value>
    /// The operation.
    /// </value>
    public ComparisonOperations Operation { get; }
}