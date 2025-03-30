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
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the discovery information.
/// </summary>
public sealed record DiscoveryInformation
{
    /// <summary>
    /// Gets or sets the dynamic client registration endpoint.
    /// </summary>
    [JsonPropertyName("dynamic_registration")]
    public Uri DynamicClientRegistrationEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the device authorization endpoint.
    /// </summary>
    [JsonPropertyName("device_authorization_endpoint")]
    public Uri DeviceAuthorizationEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the authorization end point.
    /// </summary>
    [JsonPropertyName("authorization_endpoint")]
    public Uri AuthorizationEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the check session end point.
    /// </summary>
    /// <value>
    /// The check session end point.
    /// </value>
    [JsonPropertyName("check_session_iframe")]
    public Uri CheckSessionEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of the Claim Types supported.
    /// </summary>
    [JsonPropertyName("claim_types_supported")]
    public string[] ClaimTypesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets boolean specifying whether the OP supports use of the claims parameter.
    /// </summary>
    [JsonPropertyName("claims_parameter_supported")]
    public bool ClaimsParameterSupported { get; set; }

    /// <summary>
    /// Gets or sets a list of the Claim Names of the Claims.
    /// </summary>
    [JsonPropertyName("claims_supported")]
    public string[] ClaimsSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the end session end point.
    /// </summary>
    /// <value>
    /// The end session end point.
    /// </value>
    [JsonPropertyName("end_session_endpoint")]
    public Uri EndSessionEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the grant-types supported : authorization_code, implicit
    /// </summary>
    [JsonPropertyName("grant_types_supported")]
    public string[] GrantTypesSupported { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of the JWS signing algorithms (alg values) supported.
    /// </summary>
    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public string[] IdTokenSigningAlgValuesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    [JsonPropertyName("issuer")]
    public Uri Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the JSON Web Key Set document.
    /// </summary>
    [JsonPropertyName("jwks_uri")]
    public Uri JwksUri { get; set; } = null!;

    /// <summary>
    /// Gets or sets boolean specifying whether the OP supports use of the request parameter.
    /// </summary>
    [JsonPropertyName("request_parameter_supported")]
    public bool RequestParameterSupported { get; set; }

    /// <summary>
    /// Gets or sets boolean specifying whether the OP supports use of the request request_uri
    /// </summary>
    [JsonPropertyName("request_uri_parameter_supported")]
    public bool RequestUriParameterSupported { get; set; }

    /// <summary>
    /// Gets or sets boolean specifying whether the OP requires any request_uri values.
    /// </summary>
    [JsonPropertyName("require_request_uri_registration")]
    public bool RequireRequestUriRegistration { get; set; }

    /// <summary>
    /// Gets or sets the response modes supported : query, fragment
    /// </summary>
    [JsonPropertyName("response_modes_supported")]
    public string[] ResponseModesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the response types supported : code, id_token &amp; token id_token
    /// </summary>
    [JsonPropertyName("response_types_supported")]
    public string[] ResponseTypesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the revocation end point.
    /// </summary>
    /// <value>
    /// The revocation end point.
    /// </value>
    [JsonPropertyName("revocation_endpoint")]
    public Uri RevocationEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the introspection end point.
    /// </summary>
    /// <value>
    /// The introspection end point.
    /// </value>
    [JsonPropertyName("introspection_endpoint")]
    public Uri IntrospectionEndpoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of supported scopes.
    /// </summary>
    [JsonPropertyName("scopes_supported")]
    public string[] ScopesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the subject types supported : pairwise &amp; public.
    /// </summary>
    [JsonPropertyName("subject_types_supported")]
    public string[] SubjectTypesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the token endpoint.
    /// </summary>
    [JsonPropertyName("token_endpoint")]
    public Uri TokenEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of Client Authentication methods supported by the TokenEndpoint : client_secret_post, client_secret_basic etc ...
    /// </summary>
    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public string[] TokenEndpointAuthMethodSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the user-info endpoint.
    /// </summary>
    [JsonPropertyName("userinfo_endpoint")]
    public Uri UserInfoEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the version of the discovery document
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Registration End Point.
    /// </summary>
    [JsonPropertyName("registration_endpoint")]
    public Uri RegistrationEndPoint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the acr values supported.
    /// </summary>
    public string[] AcrValuesSupported { get; set; } = ["pwd"];

    /// <summary>
    /// Gets or sets the list of the JWE encryption algorithms (alg values)
    /// </summary>
    public string[] IdTokenEncryptionAlgValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWE encryption algorithms (enc values)
    /// </summary>
    public string[] IdTokenEncryptionEncValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWS signing algorithms (alg values) supported by the UserInfo endpoint.
    /// </summary>
    public string[] UserInfoSigningAlgValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWE encryption algorithms (alg values) supported by the UserInfo endpoint.
    /// </summary>
    public string[] UserInfoEncryptionAlgValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWE encryption algorithms (enc values) supported by the UserInfo endpoint.
    /// </summary>
    public string[] UserInfoEncryptionEncValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWS signing algorithms (alg values) supported by the OP for Request objects.
    /// </summary>
    public string[] RequestObjectSigningAlgValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWE encryption algorithms (alg values) supported by the OP for Request objects.
    /// </summary>
    public string[] RequestObjectEncryptionAlgValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWE encryption algorithms (enc values) supported by the OP for Request objects.
    /// </summary>
    public string[] RequestObjectEncryptionEncValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets the list of the JWS algorithms (alg values) supported by the Token Endpoint for the signature on the JWT.
    /// </summary>
    public string[] TokenEndpointAuthSigningAlgValuesSupported { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Gets or sets a list of display parameter values.
    /// </summary>
    public string[] DisplayValuesSupported { get; set; } = [];

    /// <summary>
    /// Gets or sets the service documentation.
    /// </summary>
    public string ServiceDocumentation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the languages &amp; scripts supported for values in Claims being returned.
    /// </summary>
    public string[] ClaimsLocalesSupported { get; set; } = ["en"];

    /// <summary>
    /// Gets or sets the languages &amp; scripts supported for the UI.
    /// </summary>
    public string[] UiLocalesSupported { get; set; } = ["en"];

    /// <summary>
    /// Gets or sets the OP policy.
    /// </summary>
    public Uri? OpPolicyUri { get; set; }

    /// <summary>
    /// Gets or sets the TOS uri.
    /// </summary>
    public Uri? OpTosUri { get; set; }

    /// <summary>
    /// Gets or sets the JWS endpoint.
    /// </summary>
    /// <value>
    /// The JWS.
    /// </value>
    [JsonPropertyName("jws")]
    public Uri Jws { get; set; } = null!;

    /// <summary>
    /// Gets or sets the jwe endpoint.
    /// </summary>
    /// <value>
    /// The jwe.
    /// </value>
    [JsonPropertyName("jwe")]
    public Uri Jwe { get; set; } = null!;

    /// <summary>
    /// Gets or sets the clients endpoint.
    /// </summary>
    /// <value>
    /// The clients.
    /// </value>
    [JsonPropertyName("clients")]
    public Uri Clients { get; set; } = null!;

    /// <summary>
    /// Gets or sets the scopes endpoint.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    [JsonPropertyName("scopes")]
    public Uri Scopes { get; set; } = null!;

    /// <summary>
    /// Gets or sets the resource owners endpoint.
    /// </summary>
    /// <value>
    /// The resource owners.
    /// </value>
    [JsonPropertyName("resource_owners")]
    public Uri ResourceOwners { get; set; } = null!;

    /// <summary>
    /// Gets or sets the manage endpoint.
    /// </summary>
    /// <value>
    /// The manage.
    /// </value>
    [JsonPropertyName("manage")]
    public Uri Manage { get; set; } = null!;

    /// <summary>
    /// Gets or sets the claims endpoint.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    [JsonPropertyName("claims")]
    public Uri Claims { get; set; } = null!;
}
