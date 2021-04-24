namespace SimpleAuth.Exceptions
{
    using System;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Errors;

    /// <summary>
    /// Defines the claim required exception.
    /// </summary>
    /// <seealso cref="Exception" />
    public class ClaimRequiredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimRequiredException"/> class.
        /// </summary>
        /// <param name="claim">The claim.</param>
        public ClaimRequiredException(string claim) : base(Strings.TheClaimMustBeSpecified)
        {
            Claim = claim;
        }

        public string Code { get; } = ErrorCodes.ClaimRequired;

        /// <summary>
        /// Gets the claim.
        /// </summary>
        /// <value>
        /// The claim.
        /// </value>
        public string Claim { get; }
    }
}
