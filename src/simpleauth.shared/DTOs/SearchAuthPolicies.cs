namespace SimpleAuth.Shared.DTOs
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the search auth policy query.
    /// </summary>
    [DataContract]
    public class SearchAuthPolicies
    {
        /// <summary>
        /// Gets or sets the ids.
        /// </summary>
        /// <value>
        /// The ids.
        /// </value>
        [DataMember(Name = "ids")]
        public string[] Ids { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the resource ids.
        /// </summary>
        /// <value>
        /// The resource ids.
        /// </value>
        [DataMember(Name = "resource_ids")]
        public string[] ResourceIds { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the start index.
        /// </summary>
        /// <value>
        /// The start index.
        /// </value>
        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the total results.
        /// </summary>
        /// <value>
        /// The total results.
        /// </value>
        [DataMember(Name = "count")]
        public int TotalResults { get; set; }
    }
}
