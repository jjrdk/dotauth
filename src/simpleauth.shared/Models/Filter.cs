namespace SimpleAuth.Shared.Models
{
    /// <summary>
    /// Defines the filter content.
    /// </summary>
    public sealed class Filter
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public FilterRule[] Rules { get; set; }
    }
}
