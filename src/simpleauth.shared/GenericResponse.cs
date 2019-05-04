namespace SimpleAuth.Shared
{
    using System.Net;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the generic response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericResponse<T>
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public T Content { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status.
        /// </summary>
        /// <value>
        /// The HTTP status.
        /// </value>
        public HttpStatusCode HttpStatus { get; set; }

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
        public ErrorDetails Error { get; set; }
    }
}