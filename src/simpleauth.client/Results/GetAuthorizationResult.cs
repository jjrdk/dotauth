namespace SimpleAuth.Client.Results
{
    using System;

    /// <summary>
    /// Defines the get authorization result.
    /// </summary>
    /// <seealso cref="SimpleAuth.Client.Results.BaseSidResult" />
    public class GetAuthorizationResult : BaseSidResult
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Uri Location { get; set; }
    }
}
