namespace DotAuth.Uma;

/// <summary>
/// Defines the request denied result
/// </summary>
public record ErrorResult(string Title, string Reason) : ResourceResult;
