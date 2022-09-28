namespace SimpleAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;
using SimpleAuth.Shared.Responses;

/// <summary>
/// Defines the device authorization request
/// </summary>
public sealed record DeviceAuthorizationData
{
    /// <summary>
    /// Gets or sets the device code.
    /// </summary>
    public string DeviceCode { get; set; } = null!;

    /// <summary>
    /// Gets or sets the request client id.
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the allowed polling interval
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// Gets or sets the requested scopes.
    /// </summary>
    public string[] Scopes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether the request has been approved.
    /// </summary>
    public bool Approved { get; set; }

    /// <summary>
    /// Gets or sets the absolute request expiry time.
    /// </summary>
    public DateTimeOffset Expires { get; set; }

    /// <summary>
    /// Gets or sets the time the request was last polled.
    /// </summary>
    public DateTimeOffset LastPolled { get; set; }

    /// <summary>
    /// Gets or sets the authorization response to the client.
    /// </summary>
    public DeviceAuthorizationResponse Response { get; set; } = null!;
}

/// <summary>
/// Defines the add resource owner request.
/// </summary>
[DataContract]
public sealed record AddResourceOwnerRequest
{
    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The subject.
    /// </value>
    [DataMember(Name = "sub")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>
    /// The password.
    /// </value>
    [DataMember(Name = "password")]
    public string? Password { get; set; }
}