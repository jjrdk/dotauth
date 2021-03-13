namespace SimpleAuth.Parameters
{
    internal abstract record GrantTypeParameter
    {
        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public string? ClientId { get; init; }
        /// <summary>
        /// Gets or sets the clients secret.
        /// </summary>
        public string? ClientSecret { get; init; }
        /// <summary>
        /// Gets or sets the client assertion type
        /// </summary>
        public string? ClientAssertionType { get; init; }
        /// <summary>
        /// Gets or sets the client assertion.
        /// </summary>
        public string? ClientAssertion { get; init; }
    }
}