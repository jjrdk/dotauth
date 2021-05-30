namespace SimpleAuth.Shared.Requests
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the device authorization request
    /// </summary>
    public record DeviceAuthorizationRequest
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

        public string ClientId { get; }

        public string[] Scopes { get; }

        public IEnumerable<KeyValuePair<string?, string?>> ToForm()
        {
            yield return new KeyValuePair<string?, string?>("client_id", ClientId);
            if (Scopes.Length > 0)
            {
                yield return new KeyValuePair<string?, string?>("scope", string.Join(' ', Scopes));
            }
        }
    }
}