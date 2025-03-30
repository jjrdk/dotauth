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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the UMA configuration response.
/// </summary>
public sealed record UmaConfigurationResponse
{
    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    /// <value>
    /// The issuer.
    /// </value>
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the registration endpoint.
    /// </summary>
    /// <value>
    /// The registration endpoint.
    /// </value>
    [JsonPropertyName("registration_endpoint")]
    public string RegistrationEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token endpoint.
    /// </summary>
    /// <value>
    /// The token endpoint.
    /// </value>
    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the JWKS URI.
    /// </summary>
    /// <value>
    /// The JWKS URI.
    /// </value>
    [JsonPropertyName("jwks_uri")]
    public string JwksUri { get; set; } = null!;

    /// <summary>
    /// Gets or sets the authorization endpoint.
    /// </summary>
    /// <value>
    /// The authorization endpoint.
    /// </value>
    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the claims interaction endpoint.
    /// </summary>
    /// <value>
    /// The claims interaction endpoint.
    /// </value>
    [JsonPropertyName("claims_interaction_endpoint")]
    public string ClaimsInteractionEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the introspection endpoint.
    /// </summary>
    /// <value>
    /// The introspection endpoint.
    /// </value>
    [JsonPropertyName("introspection_endpoint")]
    public string IntrospectionEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the resource registration endpoint.
    /// </summary>
    /// <value>
    /// The resource registration endpoint.
    /// </value>
    [JsonPropertyName("resource_registration_endpoint")]
    public string ResourceRegistrationEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the permission endpoint.
    /// </summary>
    /// <value>
    /// The permission endpoint.
    /// </value>
    [JsonPropertyName("permission_endpoint")]
    public string PermissionEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the revocation endpoint.
    /// </summary>
    /// <value>
    /// The revocation endpoint.
    /// </value>
    [JsonPropertyName("revocation_endpoint")]
    public string RevocationEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the policies endpoint.
    /// </summary>
    /// <value>
    /// The policies endpoint.
    /// </value>
    [JsonPropertyName("policies_endpoint")]
    public string PoliciesEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the claim token profiles supported.
    /// </summary>
    /// <value>
    /// The claim token profiles supported.
    /// </value>
    [JsonPropertyName("claim_token_profiles_supported")]
    public List<string> ClaimTokenProfilesSupported { get; set; } = null!;

    /// <summary>
    /// Gets or sets the uma profiles supported.
    /// </summary>
    /// <value>
    /// The uma profiles supported.
    /// </value>
    [JsonPropertyName("uma_profiles_supported")]
    public List<string> UmaProfilesSupported { get; set; } = null!;

    /// <summary>
    /// Gets or sets the scopes supported.
    /// </summary>
    /// <value>
    /// The scopes supported.
    /// </value>
    [JsonPropertyName("scopes_supported")]
    public string[] ScopesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the response types supported.
    /// </summary>
    /// <value>
    /// The response types supported.
    /// </value>
    [JsonPropertyName("response_types_supported")]
    public string[] ResponseTypesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the grant types supported.
    /// </summary>
    /// <value>
    /// The grant types supported.
    /// </value>
    [JsonPropertyName("grant_types_supported")]
    public string[] GrantTypesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the token endpoint authentication methods supported.
    /// </summary>
    /// <value>
    /// The token endpoint authentication methods supported.
    /// </value>
    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public string[] TokenEndpointAuthMethodsSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the token endpoint authentication signing alg values supported.
    /// </summary>
    /// <value>
    /// The token endpoint authentication signing alg values supported.
    /// </value>
    [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
    public string[] TokenEndpointAuthSigningAlgValuesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the UI locales supported.
    /// </summary>
    /// <value>
    /// The UI locales supported.
    /// </value>
    [JsonPropertyName("ui_locales_supported")]
    public string[] UiLocalesSupported { get; set; } = [];
}
