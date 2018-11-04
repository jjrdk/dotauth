namespace SimpleIdentityServer.Uma.Core
{
    using System;

    public class UmaConfigurationOptions
    {
        public UmaConfigurationOptions(TimeSpan rptLifetime = default(TimeSpan), TimeSpan ticketLifetime = default(TimeSpan))
        {
            RptLifeTime = rptLifetime == default(TimeSpan) ? TimeSpan.FromSeconds(3600) : rptLifetime;
            TicketLifeTime = ticketLifetime == default(TimeSpan) ? TimeSpan.FromSeconds(3600) : ticketLifetime;
        }

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
