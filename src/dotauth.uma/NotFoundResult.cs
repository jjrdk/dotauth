namespace DotAuth.Uma;

using System.Runtime.Serialization;

/// <summary>
/// Defines the not found result
/// </summary>
[DataContract]
public record NotFoundResult : ResourceResult;