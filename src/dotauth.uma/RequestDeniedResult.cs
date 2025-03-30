namespace DotAuth.Uma;

using System.Runtime.Serialization;

/// <summary>
/// Defines the request denied result
/// </summary>
public record RequestDeniedResult(string Title, string Reason) : ResourceResult;
