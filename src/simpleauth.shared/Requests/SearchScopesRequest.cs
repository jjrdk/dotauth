namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the search scopes request.
    /// </summary>
    [DataContract]
    public record SearchScopesRequest
    {
        /// <summary>
        /// Gets or sets the scope types.
        /// </summary>
        /// <value>
        /// The scope types.
        /// </value>
        [DataMember(Name = "types")]
        public string[]? ScopeTypes { get; set; }

        /// <summary>
        /// Gets or sets the scope names.
        /// </summary>
        /// <value>
        /// The scope names.
        /// </value>
        [DataMember(Name = "names")]
        public string[]? ScopeNames { get; set; }

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
        public int NbResults { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SearchScopesRequest"/> is descending.
        /// </summary>
        /// <value>
        ///   <c>true</c> if descending; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "order")]
        public bool Descending { get; set; }
    }
}
