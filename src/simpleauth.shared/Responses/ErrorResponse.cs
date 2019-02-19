namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the error response.
    /// </summary>
    [DataContract]
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        [DataMember(Name = "error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the error description.
        /// </summary>
        /// <value>
        /// The error description.
        /// </value>
        [DataMember(Name = "error_description")]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Gets or sets the error URI.
        /// </summary>
        /// <value>
        /// The error URI.
        /// </value>
        [DataMember(Name = "error_uri")]
        public string ErrorUri { get; set; }
    }
}
