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
        public string[] ClientIdsAllowed { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        [DataMember(Name = "scopes")]
        public string[] Scopes { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public ClaimData[] Claims { get; set; } = Array.Empty<ClaimData>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is resource owner consent needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "consent_needed")]
        public bool IsResourceOwnerConsentNeeded { get; set; }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        [DataMember(Name = "script")]
        public string? Script { get; set; }

        /// <summary>
        /// Gets or sets the open identifier provider.
        /// </summary>
        /// <value>
        /// The open identifier provider.
        /// </value>
        [DataMember(Name = "provider")]
        public string? OpenIdProvider { get; set; }
    }
}