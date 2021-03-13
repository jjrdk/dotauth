namespace SimpleAuth.Shared.Responses
{
    using System;
    using System.Runtime.Serialization;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the policy rule view model.
    /// </summary>
    [DataContract]
    public record PolicyRuleViewModel
    {
        /// <summary>
        /// Gets or sets a comma separated string with allowed client ids.
        /// </summary>
        [DataMember(Name = "client_ids_allowed")]
        public string? ClientIdsAllowed { get; init; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        [DataMember(Name = "scopes")]
        public string? Scopes { get; init; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        [DataMember(Name = "claims")]
        public ClaimData[] Claims { get; init; } = Array.Empty<ClaimData>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is resource owner consent needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "is_resource_owner_consent_needed")]
        public bool IsResourceOwnerConsentNeeded { get; init; }

        /// <summary>
        /// Gets or sets the open identifier provider.
        /// </summary>
        [DataMember(Name = "openid_provider")]
        public string? OpenIdProvider { get; init; }
    }
}