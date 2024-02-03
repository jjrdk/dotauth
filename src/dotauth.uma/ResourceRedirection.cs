namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines an external resource location
/// </summary>
[DataContract]
public record ResourceRedirection : ResourceResult
{
    /// <summary>
    /// Gets the resource location.
    /// </summary>
    [DataMember(Name = "location")]
    public Uri Location { get; set; } = null!;
}