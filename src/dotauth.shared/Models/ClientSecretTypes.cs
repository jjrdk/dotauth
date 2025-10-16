namespace DotAuth.Shared.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the client secret types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ClientSecretTypes>))]
public enum ClientSecretTypes
{
    /// <summary>
    /// Shared secret
    /// </summary>
    [JsonStringEnumMemberName("shared_secret")]
    SharedSecret = 0,

    /// <summary>
    /// X509 thumbprint
    /// </summary>
    [JsonStringEnumMemberName("x509_thumbprint")]
    X509Thumbprint = 1,

    /// <summary>
    /// X509 name
    /// </summary>
    [JsonStringEnumMemberName("x509_name")]
    X509Name = 2
}
