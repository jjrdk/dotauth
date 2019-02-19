namespace SimpleAuth
{
    using System;
    using System.Globalization;

    public class RuntimeSettings
    {
        public RuntimeSettings(
            TimeSpan authorizationCodeValidityPeriod = default,
            string[] userClaimsToIncludeInAuthToken = null,
            string[] claimsIncludedInUserCreation = null,
            TimeSpan rptLifeTime = default,
            TimeSpan ticketLifeTime = default)
        {
            AuthorizationCodeValidityPeriod = authorizationCodeValidityPeriod == default
                ? TimeSpan.FromHours(1)
                : authorizationCodeValidityPeriod;
            UserClaimsToIncludeInAuthToken = userClaimsToIncludeInAuthToken ?? Array.Empty<string>();
            RptLifeTime = rptLifeTime == default ? TimeSpan.FromHours(1) : rptLifeTime;
            TicketLifeTime = ticketLifeTime == default ? TimeSpan.FromHours(1) : ticketLifeTime;
            ClaimsIncludedInUserCreation = claimsIncludedInUserCreation ?? Array.Empty<string>();
        }

        public TimeSpan AuthorizationCodeValidityPeriod { get; }
        public string[] UserClaimsToIncludeInAuthToken { get; }

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