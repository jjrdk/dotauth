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

namespace SimpleAuth.Shared.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the UMA configuration response.
    /// </summary>
    [DataContract]
    public class UmaConfiguration
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        [DataMember(Name = "issuer")]
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the registration endpoint.
        /// </summary>
        /// <value>
        /// The registration endpoint.
        /// </value>
        [DataMember(Name = "registration_endpoint")]
        public string RegistrationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint.
        /// </summary>
        /// <value>
        /// The token endpoint.
        /// </value>
        [DataMember(Name = "token_endpoint")]
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the JWKS URI.
        /// </summary>
        /// <value>
        /// The JWKS URI.
        /// </value>
        [DataMember(Name = "jwks_uri")]
        public string JwksUri { get; set; }

        /// <summary>
        /// Gets or sets the authorization endpoint.
        /// </summary>
        /// <value>
        /// The authorization endpoint.
        /// </value>
        [DataMember(Name = "authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the claims interaction endpoint.
        /// </summary>
        /// <value>
        /// The claims interaction endpoint.
        /// </value>
        [DataMember(Name = "claims_interaction_endpoint")]
        public string ClaimsInteractionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the introspection endpoint.
        /// </summary>
        /// <value>
        /// The introspection endpoint.
        /// </value>
        [DataMember(Name = "introspection_endpoint")]
        public string IntrospectionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the resource registration endpoint.
        /// </summary>
        /// <value>
        /// The resource registration endpoint.
        /// </value>
        [DataMember(Name = "resource_registration_endpoint")]
        public string ResourceRegistrationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the permission endpoint.
        /// </summary>
        /// <value>
        /// The permission endpoint.
        /// </value>
        [DataMember(Name = "permission_endpoint")]
        public string PermissionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the revocation endpoint.
        /// </summary>
        /// <value>
        /// The revocation endpoint.
        /// </value>
        [DataMember(Name = "revocation_endpoint")]
        public string RevocationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the policies endpoint.
        /// </summary>
        /// <value>
        /// The policies endpoint.
        /// </value>
        [DataMember(Name = "policies_endpoint")]
        public string PoliciesEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the claim token profiles supported.
        /// </summary>
        /// <value>
        /// The claim token profiles supported.
        /// </value>
        [DataMember(Name = "claim_token_profiles_supported")]
        public List<string> ClaimTokenProfilesSupported { get; set; }

        /// <summary>
        /// Gets or sets the uma profiles supported.
        /// </summary>
        /// <value>
        /// The uma profiles supported.
        /// </value>
        [DataMember(Name = "uma_profiles_supported")]
        public List<string> UmaProfilesSupported { get; set; }

        /// <summary>
        /// Gets or sets the scopes supported.
        /// </summary>
        /// <value>
        /// The scopes supported.
        /// </value>
        [DataMember(Name = "scopes_supported")]
        public List<string> ScopesSupported { get; set; }

        /// <summary>
        /// Gets or sets the response types supported.
        /// </summary>
        /// <value>
        /// The response types supported.
        /// </value>
        [DataMember(Name = "response_types_supported")]
        public List<string> ResponseTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets the grant types supported.
        /// </summary>
        /// <value>
        /// The grant types supported.
        /// </value>
        [DataMember(Name = "grant_types_supported")]
        public List<string> GrantTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint authentication methods supported.
        /// </summary>
        /// <value>
        /// The token endpoint authentication methods supported.
        /// </value>
        [DataMember(Name = "token_endpoint_auth_methods_supported")]
        public List<string> TokenEndpointAuthMethodsSupported { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint authentication signing alg values supported.
        /// </summary>
        /// <value>
        /// The token endpoint authentication signing alg values supported.
        /// </value>
        [DataMember(Name = "token_endpoint_auth_signing_alg_values_supported")]
        public List<string> TokenEndpointAuthSigningAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets the UI locales supported.
        /// </summary>
        /// <value>
        /// The UI locales supported.
        /// </value>
        [DataMember(Name = "ui_locales_supported")]
        public List<string> UiLocalesSupported { get; set; }
    }
}
