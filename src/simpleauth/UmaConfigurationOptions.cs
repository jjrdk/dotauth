namespace SimpleAuth
{
    using Shared.Models;
    using System;
    using System.Collections.Generic;

    public class UmaConfigurationOptions
    {
        public UmaConfigurationOptions(TimeSpan rptLifetime = default, TimeSpan ticketLifetime = default)
        {
            RptLifeTime = rptLifetime == default ? TimeSpan.FromSeconds(3600) : rptLifetime;
            TicketLifeTime = ticketLifetime == default ? TimeSpan.FromSeconds(3600) : ticketLifetime;
        }

        /// <summary>
        /// Gets or sets the RPT lifetime (seconds).
        /// </summary>
        public TimeSpan RptLifeTime { get; }
        /// <summary>
        /// Gets or sets the ticket lifetime (seconds).
        /// </summary>
        public TimeSpan TicketLifeTime { get; }

        public IReadOnlyCollection<ResourceSet> ResourceSets { get; set; }
        public IReadOnlyCollection<Policy> Policies { get; set; }
    }
}
