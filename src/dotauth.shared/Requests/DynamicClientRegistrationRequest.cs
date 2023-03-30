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

/// <summary>
/// Defines the dynamic client registration request.
/// </summary>
[DataContract]
public record DynamicClientRegistrationRequest
{
    /// <summary>
    /// Gets or sets the application type.
    /// </summary>
    [DataMember(Name = "application_type")]
    public string? ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the redirect uris.
    /// </summary>
    [DataMember(Name = "redirect_uris")]
    public string[] RedirectUris { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets teh client name.
    /// </summary>
    [DataMember(Name = "client_name")]
    public string? ClientName { get; set; }

    /// <summary>
    /// Gets or sets the logo uri.
    /// </summary>
    [DataMember(Name = "logo_uri")]
    public string? LogoUri { get; set; }
    
    /// <summary>
    /// Get or sets the token endpoint auth method.
    /// </summary>
    [DataMember(Name = "token_endpoint_auth_method")]
    public string? TokenEndpointAuthMethod { get; set; }

    /// <summary>
    /// Gets or sets the contacts.
    /// </summary>
    [DataMember(Name = "contacts")]
    public string[] Contacts { get; set; } = Array.Empty<string>();
}