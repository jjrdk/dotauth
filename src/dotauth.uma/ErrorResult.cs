namespace DotAuth.Uma;

using System.Runtime.Serialization;

/// <summary>
/// Defines the request denied result
/// </summary>
[DataContract]
public record ErrorResult(string Title, string Reason) : ResourceResult;