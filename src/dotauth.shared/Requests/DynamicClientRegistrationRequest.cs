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

namespace DotAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the dynamic client registration request.
/// </summary>
public record DynamicClientRegistrationRequest
{
    /// <summary>
    /// Gets or sets the application type.
    /// </summary>
    [JsonPropertyName("application_type")]
    public string? ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the redirect uris.
    /// </summary>
    [JsonPropertyName("redirect_uris")]
    public string[] RedirectUris { get; set; } = [];

    /// <summary>
    /// Gets or sets teh client name.
    /// </summary>
    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }

    /// <summary>
    /// Gets or sets the logo uri.
    /// </summary>
    [JsonPropertyName("logo_uri")]
    public string? LogoUri { get; set; }
    
    /// <summary>
    /// Get or sets the token endpoint auth method.
    /// </summary>
    [JsonPropertyName("token_endpoint_auth_method")]
    public string? TokenEndpointAuthMethod { get; set; }

    /// <summary>
    /// Gets or sets the contacts.
    /// </summary>
    [JsonPropertyName("contacts")]
    public string[] Contacts { get; set; } = [];
}
