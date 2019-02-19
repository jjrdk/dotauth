namespace SimpleAuth.Exceptions
{
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the claim required exception.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.SimpleAuthException" />
    public class ClaimRequiredException : SimpleAuthException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimRequiredException"/> class.
        /// </summary>
        /// <param name="claim">The claim.</param>
        public ClaimRequiredException(string claim)
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
