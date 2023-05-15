namespace DotAuth.Client;

using System.Runtime.Serialization;

/// <summary>
/// Defines the add resource owner response.
/// </summary>
[DataContract]
public sealed class AddResourceOwnerResponse
{
    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    [DataMember(Name = "subject")]
    public string? Subject { get; set; }
}
