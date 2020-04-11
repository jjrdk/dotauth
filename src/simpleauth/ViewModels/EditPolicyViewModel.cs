namespace SimpleAuth.ViewModels
{
    using System;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the view model for editing policies.
    /// </summary>
    public class EditPolicyViewModel
    {
        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the authorization policies.
        /// </summary>
        public PolicyRuleViewModel[] Rules { get; set; }
    }

    /// <summary>
    /// Defines the policy rule view model.
    /// </summary>
    public class PolicyRuleViewModel
    {
        /// <summary>
        /// Gets or sets a comma separated string with allowed client ids.
        /// </summary>
        public string ClientIdsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        public string Scopes { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        public ClaimData[] Claims { get; set; } = Array.Empty<ClaimData>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is resource owner consent needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
        /// </value>
        public bool IsResourceOwnerConsentNeeded { get; set; }

        /// <summary>
        /// Gets or sets the open identifier provider.
        /// </summary>
        public string OpenIdProvider { get; set; }
    }
}