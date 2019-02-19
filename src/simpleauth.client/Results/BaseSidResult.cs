namespace SimpleAuth.Client.Results
{
    using System.Net;
    using Shared.Responses;

    /// <summary>
    /// Defines the abstract base sid result.
    /// </summary>
    public abstract class BaseSidResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether [contains error].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contains error]; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsError { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public ErrorResponseWithState Error { get; set;}

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public HttpStatusCode Status { get; set; }
    }
}
