namespace DotAuth.Shared.Requests;

using System.Collections.Generic;

/// <summary>
/// Defines the device authorization request
/// </summary>
public sealed record DeviceAuthorizationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceAuthorizationRequest"/> record.
    /// </summary>
    /// <param name="clientId">The client id. Required</param>
    /// <param name="scopes">The requested scopes. Optional</param>
    public DeviceAuthorizationRequest(string clientId, params string[] scopes)
    {
        ClientId = clientId;
        Scopes = scopes;
    }

    /// <summary>
    /// Gets the client id.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// Gets the scopes.
    /// </summary>
    public string[] Scopes { get; }

    /// <summary>
    /// Returns the request as a form.
    /// </summary>
    /// <returns>The form fields.</returns>
    public IEnumerable<KeyValuePair<string?, string?>> ToForm()
    {
        yield return new KeyValuePair<string?, string?>("client_id", ClientId);
        if (Scopes.Length > 0)
        {
            yield return new KeyValuePair<string?, string?>("scope", string.Join(' ', Scopes));
        }
    }
}