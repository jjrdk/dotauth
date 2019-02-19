namespace SimpleAuth.Shared.Models
{
    /// <summary>
    /// Defines the client secret.
    /// </summary>
    public class ClientSecret
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public ClientSecretTypes Type { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }
    }
}