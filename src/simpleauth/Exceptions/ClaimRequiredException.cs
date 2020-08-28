namespace SimpleAuth.Exceptions
{
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    /// <summary>
    /// Defines the claim required exception.
    /// </summary>
    /// <seealso cref="SimpleAuthException" />
    public class ClaimRequiredException : SimpleAuthException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimRequiredException"/> class.
        /// </summary>
        /// <param name="claim">The claim.</param>
        public ClaimRequiredException(string claim) : base(ErrorCodes.ClaimRequired, Strings.TheClaimMustBeSpecified)
        {
            Claim = claim;
        }

        /// <summary>
        /// Gets the claim.
        /// </summary>
        /// <value>
        /// The claim.
        /// </value>
        public string Claim { get; }
    }
}
