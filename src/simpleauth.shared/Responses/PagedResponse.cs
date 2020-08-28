namespace SimpleAuth.Shared.Responses
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the paged response envelope.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class PagedResponse<T>
    {
        /// <summary>
        /// Gets or sets the total results.
        /// </summary>
        /// <value>
        /// The total results.
        /// </value>
        [DataMember(Name = "count")]
        public long TotalResults { get; set; }

        /// <summary>
        /// Gets or sets the start index.
        /// </summary>
        /// <value>
        /// The start index.
        /// </value>
        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [DataMember(Name = "content")]
        public T[] Content { get; set; } = Array.Empty<T>();
    }
}