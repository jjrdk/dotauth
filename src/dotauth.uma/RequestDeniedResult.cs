namespace DotAuth.Uma;

/// <summary>
/// Defines the request denied result
/// </summary>
public record RequestDeniedResult(string Title, string Reason) : ResourceResult;
