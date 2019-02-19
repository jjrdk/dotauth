namespace SimpleAuth.Shared.AccountFiltering
{
    /// <summary>
    /// Defines the filter rule content.
    /// </summary>
    public sealed class FilterRule
    {
        /// <summary>
        /// Gets or sets the claim key.
        /// </summary>
        /// <value>
        /// The claim key.
        /// </value>
        public string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets the claim value.
        /// </summary>
        /// <value>
        /// The claim value.
        /// </value>
        public string ClaimValue { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public ComparisonOperations Operation { get; set; }
    }
}
