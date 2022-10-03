namespace DotAuth.Client;

using System.Collections.Generic;

/// <summary>
/// Defines the device token request.
/// </summary>
public sealed record DeviceTokenRequest : TokenRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceTokenRequest"/> record.
    /// </summary>
    /// <param name="form">The request form.</param>
    /// <param name="interval">The polling interval.</param>
    public DeviceTokenRequest(Dictionary<string, string> form, int interval) : base(form)
    {
        Interval = interval;
    }

    /// <summary>
    /// Gets the polling interval.
    /// </summary>
    public int Interval { get; }
}