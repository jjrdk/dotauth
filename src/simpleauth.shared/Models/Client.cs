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

namespace SimpleAuth.Shared.Models
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Runtime.Serialization;
    using System.Security.Claims;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the client.
    /// </summary>
    [DataContract]
    public class Client
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        [DataMember(Name = "client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secrets.
        /// </summary>
        [DataMember(Name = "secrets")]
        public ClientSecret[] Secrets { get; set; } = Array.Empty<ClientSecret>();

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        [DataMember(Name = "client_name")]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the user claims to include in authorization token.
        /// </summary>
        /// <value>
        /// The user claims to include in authentication token.
        /// </value>
        [DataMember(Name = "included_user_claims")]
        public Regex[] UserClaimsToIncludeInAuthToken { get; set; } = Array.Empty<Regex>();
        
        /// <summary>
        /// Gets or sets the logo uri
        /// </summary>
        [DataMember(Name = "logo_uri")]
        public Uri LogoUri { get; set; }

        /// <summary>
        /// Gets or sets the token lifetime.
        /// </summary>
        /// <value>
        /// The token lifetime.
        /// </value>
        [DataMember(Name = "token_lifetime")]
        public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Gets or sets the home page of the client.
        /// </summary>
        [DataMember(Name = "client_uri")]
        public Uri ClientUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the how the profile data will be used.
        /// </summary>
        [DataMember(Name = "policy_uri")]
        public Uri PolicyUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the RP's terms of service.
        /// </summary>
        [DataMember(Name = "tos_uri")]
        public Uri TosUri { get; set; }

        /// <summary>
        /// Gets or sets the JWS alg algorithm for signing the ID token issued to this client.
        /// The default is RS256. The public key for validating the signature is provided by retrieving the JWK Set referenced by the JWKS_URI
        /// </summary>
        [DataMember(Name = "id_token_signed_response_alg")]
        public string IdTokenSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE alg algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// The default is that no encryption is performed
        /// </summary>
        [DataMember(Name = "id_token_encrypted_response_alg")]
        public string IdTokenEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE enc algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// If IdTokenEncryptedResponseAlg is specified then the value is A128CBC-HS256
        /// </summary>
        [DataMember(Name = "id_token_encrypted_response_enc")]
        public string IdTokenEncryptedResponseEnc { get; set; }

        /// <summary>
        /// Gets or sets the client authentication method for the Token Endpoint.
        /// </summary>
        [DataMember(Name = "token_endpoint_auth_method")]
        public string TokenEndPointAuthMethod { get; set; } = TokenEndPointAuthenticationMethods.ClientSecretBasic;

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 response_type values
        /// </summary>
        [DataMember(Name = "response_types")]
        public string[] ResponseTypes { get; set; } = { ResponseTypeNames.Code };

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 grant types
        /// </summary>
        [DataMember(Name = "grant_types")]
        public string[] GrantTypes { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets a list of OAUTH2.0 grant_types.
        /// </summary>
        [DataMember(Name = "allowed_scopes")]
        public string[] AllowedScopes { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets an array of Redirection URI values used by the client.
        /// </summary>
        [DataMember(Name = "redirect_uris")]
        public Uri[] RedirectionUrls { get; set; } = Array.Empty<Uri>();

        /// <summary>
        /// Gets or sets the type of application
        /// </summary>
        [DataMember(Name = "application_type")]
        public string ApplicationType { get; set; } = ApplicationTypes.Web;

        ///// <summary>
        ///// Url for the Client's JSON Web Key Set document
        ///// </summary>
        //[DataMember(Name = "jwks_uri")]
        //public Uri JwksUri { get; set; }

        /// <summary>
        /// Gets or sets the list of json web keys
        /// </summary>
        [DataMember(Name = "jwks")]
        public JsonWebKeySet JsonWebKeys { get; set; } = new JsonWebKeySet();

        /// <summary>
        /// Gets or sets the list of contacts
        /// </summary>
        [DataMember(Name = "contacts")]
        public string[] Contacts { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public Claim[] Claims { get; set; } = Array.Empty<Claim>();

        /// <summary>
        /// Get or set the sector identifier uri
        /// </summary>
        [DataMember(Name = "sector_identifier_uri")]
        public Uri SectorIdentifierUri { get; set; }

        ///// <summary>
        ///// Gets or sets the subject type
        ///// </summary>
        //[DataMember(Name = "subject_type")]
        //public string SubjectType { get; set; }

        /// <summary>
        /// Gets or sets the user info signed response algorithm
        /// </summary>
        [DataMember(Name = "userinfo_signed_response_alg")]
        public string UserInfoSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response algorithm
        /// </summary>
        [DataMember(Name = "userinfo_encrypted_response_alg")]
        public string UserInfoEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response enc
        /// </summary>
        [DataMember(Name = "userinfo_encrypted_response_enc")]
        public string UserInfoEncryptedResponseEnc { get; set; }

        /// <summary>
        /// Gets or sets the request objects signing algorithm
        /// </summary>
        [DataMember(Name = "request_object_signing_alg")]
        public string RequestObjectSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption algorithm
        /// </summary>
        [DataMember(Name = "request_object_encryption_alg")]
        public string RequestObjectEncryptionAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption enc
        /// </summary>
        [DataMember(Name = "request_object_encryption_enc")]
        public string RequestObjectEncryptionEnc { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint authentication signing algorithm
        /// </summary>
        [DataMember(Name = "token_endpoint_auth_signing_alg")]
        public string TokenEndPointAuthSigningAlg { get; set; } = SecurityAlgorithms.RsaSha256;

        ///// <summary>
        ///// Gets or sets the default max age
        ///// </summary>
        //[DataMember(Name = "default_max_age")]
        //public double DefaultMaxAge { get; set; }

        /// <summary>
        /// Gets or sets the require authentication time
        /// </summary>
        [DataMember(Name = "require_auth_time")]
        public bool RequireAuthTime { get; set; }

        /// <summary>
        /// Gets or sets the default acr values
        /// </summary>
        [DataMember(Name = "default_acr_values")]
        public string DefaultAcrValues { get; set; }

        /// <summary>
        /// Gets or sets the initiate login uri
        /// </summary>
        [DataMember(Name = "initiate_login_uri")]
        public Uri InitiateLoginUri { get; set; }

        /// <summary>
        /// Gets or sets the list of request uris
        /// </summary>
        [DataMember(Name = "request_uris")]
        public Uri[] RequestUris { get; set; } = Array.Empty<Uri>();

        /// <summary>
        /// Client require PKCE.
        /// </summary>
        [DataMember(Name = "require_pkce")]
        public bool RequirePkce { get; set; }

        /// <summary>
        /// Get or sets the post logout redirect uris.
        /// </summary>
        [DataMember(Name = "post_logout_redirect_uris")]
        public Uri[] PostLogoutRedirectUris { get; set; } = Array.Empty<Uri>();
    }
}
