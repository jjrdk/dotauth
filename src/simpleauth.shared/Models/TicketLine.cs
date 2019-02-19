namespace SimpleAuth.Shared.Models
{
    /// <summary>
    /// Defines the ticket line content.
    /// </summary>
    public class TicketLine
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets the resource set identifier.
        /// </summary>
        /// <value>
        /// The resource set identifier.
        /// </value>
        public string ResourceSetId { get; set; }
    }
}
