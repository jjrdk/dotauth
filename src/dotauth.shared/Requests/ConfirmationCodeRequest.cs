namespace DotAuth.Shared.Requests;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the confirmation code request.
/// </summary>
public sealed record ConfirmationCodeRequest
{
    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    /// <value>
    /// The phone number.
    /// </value>
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }
}
