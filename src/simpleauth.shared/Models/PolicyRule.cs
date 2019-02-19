namespace SimpleAuth.Shared.Models
{
    using System.Security.Claims;

    /// <summary>
    /// Defines the policy rule content.
    /// </summary>
    public class PolicyRule
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the client ids allowed.
        /// </summary>
        /// <value>
        /// The client ids allowed.
        /// </value>
        public string[] ClientIdsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public Claim[] Claims { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is resource owner consent needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
        /// </value>
        public bool IsResourceOwnerConsentNeeded { get; set; }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets the open identifier provider.
        /// </summary>
        /// <value>
        /// The open identifier provider.
        /// </value>
        public string OpenIdProvider { get; set; }
    }
}