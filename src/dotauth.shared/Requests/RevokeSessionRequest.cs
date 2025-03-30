namespace DotAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the revoke session request.
/// </summary>
public sealed record RevokeSessionRequest
{
    /// <summary>
    /// Gets or sets the identifier token hint.
    /// </summary>
    /// <value>
    /// The identifier token hint.
    /// </value>
    [JsonPropertyName("id_token_hint")]
#pragma warning disable IDE1006 // Naming Styles
    public string? id_token_hint { get; set; }

    /// <summary>
    /// Gets or sets the post logout redirect URI.
    /// </summary>
    /// <value>
    /// The post logout redirect URI.
    /// </value>
    [JsonPropertyName("post_logout_redirect_uri")]
    public Uri? post_logout_redirect_uri { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    /// <value>
    /// The state.
    /// </value>
    [JsonPropertyName("state")]
    public string? state { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
