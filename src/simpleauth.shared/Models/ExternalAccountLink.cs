namespace SimpleAuth.Shared.Models
{
    using System;
    using System.Security.Claims;

    /// <summary>
    /// Defines the external link content.
    /// </summary>
    public record ExternalAccountLink
    {
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; init; } = null!;

        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; init; } = null!;

        /// <summary>
        /// Gets or sets the external claims.
        /// </summary>
        /// <value>
        /// The external claims.
        /// </value>
        public Claim[] ExternalClaims { get; init; } = Array.Empty<Claim>();
    }
}