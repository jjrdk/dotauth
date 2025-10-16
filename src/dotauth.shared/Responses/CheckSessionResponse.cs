namespace DotAuth.Shared.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the check session response.
/// </summary>
public sealed record CheckSessionResponse
{
    /// <summary>
    /// Gets or sets the cookie name.
    /// </summary>
    [JsonPropertyName("cookie_name")]
    public string CookieName { get; set; } = null!;
}
