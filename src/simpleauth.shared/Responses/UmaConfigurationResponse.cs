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

    [DataContract]
    public class UmaConfigurationResponse
    {
        [DataMember(Name = "issuer")]
        public string Issuer { get; set; }
        [DataMember(Name = "registration_endpoint")]
        public string RegistrationEndpoint { get; set; }
        [DataMember(Name = "token_endpoint")]
        public string TokenEndpoint { get; set; }
        [DataMember(Name = "jwks_uri")]
        public string JwksUri { get; set; }
        [DataMember(Name = "authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }
        [DataMember(Name = "claims_interaction_endpoint")]
        public string ClaimsInteractionEndpoint { get; set; }
        [DataMember(Name = "introspection_endpoint")]
        public string IntrospectionEndpoint { get; set; }
        [DataMember(Name = "resource_registration_endpoint")]
        public string ResourceRegistrationEndpoint { get; set; }
        [DataMember(Name = "permission_endpoint")]
        public string PermissionEndpoint { get; set; }
        [DataMember(Name = "revocation_endpoint")]
        public string RevocationEndpoint { get; set; }
        [DataMember(Name = "policies_endpoint")]
        public string PoliciesEndpoint { get; set; }
        [DataMember(Name = "claim_token_profiles_supported")]
        public List<string> ClaimTokenProfilesSupported { get; set; }
        [DataMember(Name = "uma_profiles_supported")]
        public List<string> UmaProfilesSupported { get; set; }
        [DataMember(Name = "scopes_supported")]
        public List<string> ScopesSupported { get; set; }
        [DataMember(Name = "response_types_supported")]
        public List<string> ResponseTypesSupported { get; set; }
        [DataMember(Name = "grant_types_supported")]
        public List<string> GrantTypesSupported { get; set; }
        [DataMember(Name = "token_endpoint_auth_methods_supported")]
        public List<string> TokenEndpointAuthMethodsSupported { get; set; }
        [DataMember(Name = "token_endpoint_auth_signing_alg_values_supported")]
        public List<string> TokenEndpointAuthSigningAlgValuesSupported { get; set; }
        [DataMember(Name = "ui_locales_supported")]
        public List<string> UiLocalesSupported { get; set; }
    }
}
