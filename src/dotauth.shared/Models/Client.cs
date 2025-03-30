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

namespace DotAuth.Shared.Models;

using System;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the client.
/// </summary>
public class Client
{
    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client secrets.
    /// </summary>
    [JsonPropertyName("secrets")]
    public ClientSecret[] Secrets { get; set; } = [];

    /// <summary>
    /// Gets or sets the name of the client.
    /// </summary>
    /// <value>
    /// The name of the client.
    /// </value>
    [JsonPropertyName("client_name")]
    public string ClientName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user claims to include in authorization token.
    /// </summary>
    /// <value>
    /// The user claims to include in authentication token.
    /// </value>
    [JsonPropertyName("included_user_claims")]
    public Regex[] UserClaimsToIncludeInAuthToken { get; set; } = [];

    /// <summary>
    /// Gets or sets the logo uri
    /// </summary>
    [JsonPropertyName("logo_uri")]
    public Uri? LogoUri { get; set; }

    /// <summary>
    /// Gets or sets the token lifetime.
    /// </summary>
    /// <value>
    /// The token lifetime.
    /// </value>
    [JsonPropertyName("token_lifetime")]
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the home page of the client.
    /// </summary>
    [JsonPropertyName("client_uri")]
    public Uri? ClientUri { get; set; }

    /// <summary>
    /// Gets or sets the URL that the RP provides to the End-User to read about the how the profile data will be used.
    /// </summary>
    [JsonPropertyName("policy_uri")]
    public Uri? PolicyUri { get; set; }

    /// <summary>
    /// Gets or sets the URL that the RP provides to the End-User to read about the RP's terms of service.
    /// </summary>
    [JsonPropertyName("tos_uri")]
    public Uri? TosUri { get; set; }

    /// <summary>
    /// Gets or sets the JWS alg algorithm for signing the ID token issued to this client.
    /// The default is RS256. The public key for validating the signature is provided by retrieving the JWK Set referenced by the JWKS_URI
    /// </summary>
    [JsonPropertyName("id_token_signed_response_alg")]
    public string? IdTokenSignedResponseAlg { get; set; }

    /// <summary>
    /// Gets or sets the JWE alg algorithm. REQUIRED for encrypting the ID token issued to this client.
    /// The default is that no encryption is performed
    /// </summary>
    [JsonPropertyName("id_token_encrypted_response_alg")]
    public string? IdTokenEncryptedResponseAlg { get; set; }

    /// <summary>
    /// Gets or sets the JWE enc algorithm. REQUIRED for encrypting the ID token issued to this client.
    /// If IdTokenEncryptedResponseAlg is specified then the value is A128CBC-HS256
    /// </summary>
    [JsonPropertyName("id_token_encrypted_response_enc")]
    public string? IdTokenEncryptedResponseEnc { get; set; }

    /// <summary>
    /// Gets or sets the client authentication method for the Token Endpoint.
    /// </summary>
    [JsonPropertyName("token_endpoint_auth_method")]
    public string TokenEndPointAuthMethod { get; set; } = TokenEndPointAuthenticationMethods.ClientSecretBasic;

    /// <summary>
    /// Gets or sets an array containing a list of OAUTH2.0 response_type values
    /// </summary>
    [JsonPropertyName("response_types")]
    public string[] ResponseTypes { get; set; } = [ResponseTypeNames.Code];

    /// <summary>
    /// Gets or sets an array containing a list of OAUTH2.0 grant types
    /// </summary>
    [JsonPropertyName("grant_types")]
    public string[] GrantTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of OAUTH2.0 grant_types.
    /// </summary>
    [JsonPropertyName("allowed_scopes")]
    public string[] AllowedScopes { get; set; } = [];

    /// <summary>
    /// Gets or sets an array of Redirection URI values used by the client.
    /// </summary>
    [JsonPropertyName("redirect_uris")]
    public Uri[] RedirectionUrls { get; set; } = [];

    /// <summary>
    /// Gets or sets the type of application
    /// </summary>
    [JsonPropertyName("application_type")]
    public string ApplicationType { get; set; } = ApplicationTypes.Web;

    ///// <summary>
    ///// Url for the Client's JSON Web Key Set document
    ///// </summary>
    //[JsonPropertyName("jwks_uri")]
    //public Uri JwksUri { get; set; }

    /// <summary>
    /// Gets or sets the list of json web keys
    /// </summary>
    [JsonPropertyName("jwks")]
    public JsonWebKeySet? JsonWebKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of contacts
    /// </summary>
    [JsonPropertyName("contacts")]
    public string[] Contacts { get; set; } = [];

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    [JsonPropertyName("claims")]
    public Claim[] Claims { get; set; } = [];

    /// <summary>
    /// Get or set the sector identifier uri
    /// </summary>
    [JsonPropertyName("sector_identifier_uri")]
    public Uri? SectorIdentifierUri { get; set; }

    ///// <summary>
    ///// Gets or sets the subject type
    ///// </summary>
    //[JsonPropertyName("subject_type")]
    //public string SubjectType { get; set; }

    /// <summary>
    /// Gets or sets the user info signed response algorithm
    /// </summary>
    [JsonPropertyName("userinfo_signed_response_alg")]
    public string? UserInfoSignedResponseAlg { get; set; }

    /// <summary>
    /// Gets or sets the user info encrypted response algorithm
    /// </summary>
    [JsonPropertyName("userinfo_encrypted_response_alg")]
    public string? UserInfoEncryptedResponseAlg { get; set; }

    /// <summary>
    /// Gets or sets the user info encrypted response enc
    /// </summary>
    [JsonPropertyName("userinfo_encrypted_response_enc")]
    public string? UserInfoEncryptedResponseEnc { get; set; }

    /// <summary>
    /// Gets or sets the request objects signing algorithm
    /// </summary>
    [JsonPropertyName("request_object_signing_alg")]
    public string? RequestObjectSigningAlg { get; set; }

    /// <summary>
    /// Gets or sets the request object encryption algorithm
    /// </summary>
    [JsonPropertyName("request_object_encryption_alg")]
    public string? RequestObjectEncryptionAlg { get; set; }

    /// <summary>
    /// Gets or sets the request object encryption enc
    /// </summary>
    [JsonPropertyName("request_object_encryption_enc")]
    public string? RequestObjectEncryptionEnc { get; set; }

    /// <summary>
    /// Gets or sets the token endpoint authentication signing algorithm
    /// </summary>
    [JsonPropertyName("token_endpoint_auth_signing_alg")]
    public string TokenEndPointAuthSigningAlg { get; set; } = SecurityAlgorithms.RsaSha256;

    /// <summary>
    /// Gets or sets the default max age
    /// </summary>
    [JsonPropertyName("default_max_age")]
    public double DefaultMaxAge { get; set; }

    /// <summary>
    /// Gets or sets the require authentication time
    /// </summary>
    [JsonPropertyName("require_auth_time")]
    public bool RequireAuthTime { get; set; }

    /// <summary>
    /// Gets or sets the default acr values
    /// </summary>
    [JsonPropertyName("default_acr_values")]
    public string? DefaultAcrValues { get; set; }

    /// <summary>
    /// Gets or sets the initiate login uri
    /// </summary>
    [JsonPropertyName("initiate_login_uri")]
    public Uri? InitiateLoginUri { get; set; }

    /// <summary>
    /// Client require PKCE.
    /// </summary>
    [JsonPropertyName("require_pkce")]
    public bool RequirePkce { get; set; }

    /// <summary>
    /// Get or sets the post logout redirect uris.
    /// </summary>
    [JsonPropertyName("post_logout_redirect_uris")]
    public Uri[] PostLogoutRedirectUris { get; set; } = [];
}
