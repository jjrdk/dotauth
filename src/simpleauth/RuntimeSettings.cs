namespace SimpleAuth
{
    using System;
    using System.Text.RegularExpressions;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the runtime settings.
    /// </summary>
    public class RuntimeSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeSettings"/> class.
        /// </summary>
        /// <param name="onResourceOwnerCreated"></param>
        /// <param name="authorizationCodeValidityPeriod">The authorization code validity period.</param>
        /// <param name="userClaimsToIncludeInAuthToken">The user claims to include in authentication token.</param>
        /// <param name="claimsIncludedInUserCreation">The claims included in user creation.</param>
        /// <param name="rptLifeTime">The RPT life time.</param>
        /// <param name="ticketLifeTime">The ticket life time.</param>
        public RuntimeSettings(
            Action<ResourceOwner> onResourceOwnerCreated = null,
            TimeSpan authorizationCodeValidityPeriod = default,
            Regex[] userClaimsToIncludeInAuthToken = null,
            string[] claimsIncludedInUserCreation = null,
            TimeSpan rptLifeTime = default,
            TimeSpan ticketLifeTime = default)
        {
            OnResourceOwnerCreated = onResourceOwnerCreated ?? (r => { });
            AuthorizationCodeValidityPeriod = authorizationCodeValidityPeriod == default
                ? TimeSpan.FromHours(1)
                : authorizationCodeValidityPeriod;
            UserClaimsToIncludeInAuthToken = userClaimsToIncludeInAuthToken ?? Array.Empty<Regex>();
            RptLifeTime = rptLifeTime == default ? TimeSpan.FromHours(1) : rptLifeTime;
            TicketLifeTime = ticketLifeTime == default ? TimeSpan.FromHours(1) : ticketLifeTime;
            ClaimsIncludedInUserCreation = claimsIncludedInUserCreation ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the delegate to run when resource owner created.
        /// </summary>
        /// <value>
        /// The on resource owner created.
        /// </value>
        public Action<ResourceOwner> OnResourceOwnerCreated { get; }

        /// <summary>
        /// Gets the authorization code validity period.
        /// </summary>
        /// <value>
        /// The authorization code validity period.
        /// </value>
        public TimeSpan AuthorizationCodeValidityPeriod { get; }

        /// <summary>
        /// Gets the user claims to include in authentication token.
        /// </summary>
        /// <value>
        /// The user claims to include in authentication token.
        /// </value>
        public Regex[] UserClaimsToIncludeInAuthToken { get; }

        /// <summary>
        /// Gets a list of claims include when the resource owner is created.
        /// If the list is empty then all the claims are included.
        /// </summary>
        public string[] ClaimsIncludedInUserCreation { get; }

        /// <summary>
        /// Gets or sets the RPT lifetime (seconds).
        /// </summary>
        public TimeSpan RptLifeTime { get; }

        /// <summary>
        /// Gets or sets the ticket lifetime (seconds).
        /// </summary>
        public TimeSpan TicketLifeTime { get; }
    }
}
