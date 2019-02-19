﻿namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;
    using System.Security.Claims;

    /// <summary>
    /// Defines the update policy response.
    /// </summary>
    [DataContract]
    public class PolicyRuleResponse
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the client ids allowed.
        /// </summary>
        /// <value>
        /// The client ids allowed.
        /// </value>
        [DataMember(Name = "clients")]
        public string[] ClientIdsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        [DataMember(Name = "scopes")]
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public Claim[] Claims { get; set; }

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
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets the open identifier provider.
        /// </summary>
        /// <value>
        /// The open identifier provider.
        /// </value>
        [DataMember(Name = "provider")]
        public string OpenIdProvider { get; set; }
    }
}