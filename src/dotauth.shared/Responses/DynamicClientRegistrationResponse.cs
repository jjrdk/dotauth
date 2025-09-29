// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DotAuth.Shared.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the dynamic client registration response.
/// </summary>
public sealed record DynamicClientRegistrationResponse
{
    /// <summary>
    /// Gets or sets the client id.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client secret expires at.
    /// </summary>
    [JsonPropertyName("client_secret_expires_at")]
    public int ClientSecretExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the registration access token.
    /// </summary>
    [JsonPropertyName("registration_access_token")]
    public string RegistrationAccessToken { get; set; } = null!;

    /// <summary>
    /// Gets or sets the registration client uri.
    /// </summary>
    [JsonPropertyName("registration_client_uri")]
    public string RegistrationClientUri { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token endpoint auth method.
    /// </summary>
    [JsonPropertyName("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; } = null!;

    /// <summary>
    /// Gets or sets the application type.
    /// </summary>
    [JsonPropertyName("application_type")]
    public string ApplicationType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the redirect uris.
    /// </summary>
    [JsonPropertyName("redirect_uris")]
    public string[] RedirectUris { get; set; } = [];

    /// <summary>
    /// Gets or sets the client name.
    /// </summary>
    [JsonPropertyName("client_name")]
    public string ClientName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the contacts.
    /// </summary>
    [JsonPropertyName("contacts")]
    public string[] Contacts { get; set; } = [];
}
