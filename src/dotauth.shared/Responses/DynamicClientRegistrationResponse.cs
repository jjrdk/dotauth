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

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the dynamic client registration response.
/// </summary>
[DataContract]
public sealed record DynamicClientRegistrationResponse
{
    /// <summary>
    /// Gets or sets the client id.
    /// </summary>
    [DataMember(Name = "client_id")]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    [DataMember(Name = "client_secret")]
    public string ClientSecret { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client secret expires at.
    /// </summary>
    [DataMember(Name = "client_secret_expires_at")]
    public int ClientSecretExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the registration access token.
    /// </summary>
    [DataMember(Name = "registration_access_token")]
    public string RegistrationAccessToken { get; set; } = null!;

    /// <summary>
    /// Gets or sets the registration client uri.
    /// </summary>
    [DataMember(Name = "registration_client_uri")]
    public string RegistrationClientUri { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token endpoint auth method.
    /// </summary>
    [DataMember(Name = "token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; } = null!;

    /// <summary>
    /// Gets or sets the application type.
    /// </summary>
    [DataMember(Name = "application_type")]
    public string ApplicationType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the redirect uris.
    /// </summary>
    [DataMember(Name = "redirect_uris")]
    public string[] RedirectUris { get; set; } = [];

    /// <summary>
    /// Gets or sets the client name.
    /// </summary>
    [DataMember(Name = "client_name")]
    public string ClientName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the contacts.
    /// </summary>
    [DataMember(Name = "contacts")]
    public string[] Contacts { get; set; } = [];
}
