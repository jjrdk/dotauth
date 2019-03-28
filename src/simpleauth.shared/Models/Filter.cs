namespace SimpleAuth.Shared.Models
{
    using System;

    /// <summary>
    /// Defines the filter content.
    /// </summary>
    public sealed class Filter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        public Filter(string name, params FilterRule[] rules)
        {
            Name = name ?? string.Empty;
            Rules = rules ?? Array.Empty<FilterRule>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public FilterRule[] Rules { get; }
    }
}
