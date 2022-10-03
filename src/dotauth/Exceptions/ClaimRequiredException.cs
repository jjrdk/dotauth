namespace DotAuth.Exceptions;

using System;
using DotAuth.Properties;
using DotAuth.Shared.Errors;

/// <summary>
/// Defines the claim required exception.
/// </summary>
/// <seealso cref="Exception" />
public sealed class ClaimRequiredException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimRequiredException"/> class.
    /// </summary>
    /// <param name="claim">The claim.</param>
    public ClaimRequiredException(string claim) : base(Strings.TheClaimMustBeSpecified)
    {
        Claim = claim;
    }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; } = ErrorCodes.ClaimRequired;

    /// <summary>
    /// Gets the claim.
    /// </summary>
    /// <value>
    /// The claim.
    /// </value>
    public string Claim { get; }
}