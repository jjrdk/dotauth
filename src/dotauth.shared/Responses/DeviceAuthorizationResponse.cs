namespace DotAuth.Shared.Responses;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the device authorization response.
/// </summary>
public sealed record DeviceAuthorizationResponse
{
    /// <summary>
    /// REQUIRED. The device verification code.
    /// </summary>
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; } = null!;

    /// <summary>
    /// REQUIRED. The end-user verification code.
    /// </summary>
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; } = null!;

    /// <summary>
    /// REQUIRED. The end-user verification URI on the authorization server.
    /// The URI should be short and easy to remember as end users will be asked to manually type it into their user agent.
    /// </summary>
    [JsonPropertyName("verification_uri")]
    public string VerificationUri { get; set; } = null!;

    /// <summary>
    /// OPTIONAL. A verification URI that includes the "user_code" (or other information with the same function as the "user_code"), which is designed for non-textual transmission.
    /// </summary>
    [JsonPropertyName("verification_uri_complete")]
    public string VerificationUriComplete { get; set; } = null!;

    /// <summary>
    /// REQUIRED.The lifetime in seconds of the "device_code" and "user_code".
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// OPTIONAL. The minimum amount of time in seconds that the client SHOULD wait between polling requests to the token endpoint.If no value is provided, clients MUST use 5 as the default.
    /// </summary>
    [JsonPropertyName("interval")]
    public int Interval { get; set; }
}
