namespace SimpleIdentityServer.Core.Common.AccountFiltering
{
    using System;
    using System.Collections.Generic;

    public sealed class Filter
    {
        public string Name { get; set; }
        public IEnumerable<FilterRule> Rules { get; set; }
    }
}
