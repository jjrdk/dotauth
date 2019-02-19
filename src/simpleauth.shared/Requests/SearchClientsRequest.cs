namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the client search request.
    /// </summary>
    [DataContract]
    public class SearchClientsRequest
    {
        /// <summary>
        /// Gets or sets the client names.
        /// </summary>
        /// <value>
        /// The client names.
        /// </value>
        [DataMember(Name = "client_names")]
        public string[] ClientNames { get; set; }

        /// <summary>
        /// Gets or sets the client ids.
        /// </summary>
        /// <value>
        /// The client ids.
        /// </value>
        [DataMember(Name = "client_ids")]
        public string[] ClientIds { get; set; }

        /// <summary>
        /// Gets or sets the client types.
        /// </summary>
        /// <value>
        /// The client types.
        /// </value>
        [DataMember(Name = "client_types")]
        public ApplicationTypes[] ClientTypes { get; set; }

        /// <summary>
        /// Gets or sets the start index.
        /// </summary>
        /// <value>
        /// The start index.
        /// </value>
        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the nb results.
        /// </summary>
        /// <value>
        /// The nb results.
        /// </value>
        [DataMember(Name = "count")]
        public int NbResults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SearchClientsRequest"/> is descending.
        /// </summary>
        /// <value>
        ///   <c>true</c> if descending; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "order")]
        public bool Descending { get; set; }
    }
}
