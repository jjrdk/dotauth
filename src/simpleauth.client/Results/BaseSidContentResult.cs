namespace SimpleAuth.Client.Results
{
    /// <summary>
    /// Defines the base sid content result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="SimpleAuth.Client.Results.BaseSidResult" />
    public class BaseSidContentResult<T> : BaseSidResult
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public T Content { get; set; }
    }
}