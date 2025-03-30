namespace DotAuth.Shared.Responses;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the introspection response.
/// </summary>
public abstract record IntrospectionResponse
{
    /// <summary>
    /// Gets or sets a boolean indicator of whether or not the presented token is currently active
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the client id
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets or sets identifier for the resource owner who authorized this token
    /// </summary>
    [JsonPropertyName("username")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token type
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }= null!;

    /// <summary>
    /// Gets or sets the expiration in seconds
    /// </summary>
    [JsonPropertyName("exp")]
    public int Expiration { get; set; }

    /// <summary>
    /// Gets or sets the issue date
    /// </summary>
    [JsonPropertyName("iat")]
    public double IssuedAt { get; set; }

    /// <summary>
    /// Gets or sets the NBF
    /// </summary>
    [JsonPropertyName("nbf")]
    public int Nbf { get; set; }

    /// <summary>
    /// Gets or sets the subject
    /// </summary>
    [JsonPropertyName("sub")]
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Gets or sets the audience
    /// </summary>
    [JsonPropertyName("aud")]
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Gets or sets the issuer of this token
    /// </summary>
    [JsonPropertyName("iss")]
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the string representing the issuer of the token
    /// </summary>
    [JsonPropertyName("jti")]
    public string? Jti { get; set; }
}
