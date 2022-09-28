namespace SimpleAuth.Shared.Requests;

using System.Runtime.Serialization;

/// <summary>
/// Defines the request for resource owner searches.
/// </summary>
[DataContract]
public sealed record SearchResourceOwnersRequest
{
    /// <summary>
    /// Gets or sets the subjects.
    /// </summary>
    /// <value>
    /// The subjects.
    /// </value>
    [DataMember(Name = "subjects")]
    public string[]? Subjects { get; set; }

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
    /// Gets or sets a value indicating whether this <see cref="SearchResourceOwnersRequest"/> is descending.
    /// </summary>
    /// <value>
    ///   <c>true</c> if descending; otherwise, <c>false</c>.
    /// </value>
    [DataMember(Name = "order")]
    public bool Descending { get; set; }
}