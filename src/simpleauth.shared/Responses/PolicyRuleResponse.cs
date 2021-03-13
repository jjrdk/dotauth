namespace SimpleAuth.Shared.Responses
{
    using System;
    using System.Runtime.Serialization;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the update policy response.
    /// </summary>
    [DataContract]
    public record PolicyRuleResponse
    {
        /// <summary>
        /// Gets or sets the client ids allowed.
        /// </summary>
        /// <value>
        /// The client ids allowed.
        /// </value>
        [DataMember(Name = "clients")]
        public string[] ClientIdsAllowed { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        [DataMember(Name = "scopes")]
        public string[] Scopes { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public ClaimData[] Claims { get; init; } = Array.Empty<ClaimData>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is resource owner consent needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "consent_needed")]
        public bool IsResourceOwnerConsentNeeded { get; init; }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        [DataMember(Name = "script")]
        public string? Script { get; init; }

        /// <summary>
        /// Gets or sets the open identifier provider.
        /// </summary>
        /// <value>
        /// The open identifier provider.
        /// </value>
        [DataMember(Name = "provider")]
        public string? OpenIdProvider { get; init; }
    }
}