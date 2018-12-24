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
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Requests;

    [DataContract]
    public class ClientResponse
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.ClientId)]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.Secrets)]
        public IEnumerable<ResponseClientSecret> Secrets { get; set; }

        /// <summary>
        /// Gets or sets the client name
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.ClientName)]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the logo uri
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.LogoUri)]
        public string LogoUri { get; set; }

        /// <summary>
        /// Gets or sets the home page of the client.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.ClientUri)]
        public string ClientUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the how the profile data will be used.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.PolicyUri)]
        public string PolicyUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the RP's terms of service.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.TosUri)]
        public string TosUri { get; set; }

        /// <summary>
        /// Gets or sets the JWS alg algorithm for signing the ID token issued to this client.
        /// The default is RS256. The public key for validating the signature is provided by retrieving the JWK Set referenced by the JWKS_URI
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.IdTokenSignedResponseAlg)]
        public string IdTokenSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE alg algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// The default is that no encryption is performed
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.IdTokenEncryptedResponseAlg)]
        public string IdTokenEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE enc algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// If IdTokenEncryptedResponseAlg is specified then the value is A128CBC-HS256
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.IdTokenEncryptedResponseEnc)]
        public string IdTokenEncryptedResponseEnc { get; set; }

        /// <summary>
        /// Gets or sets the client authentication method for the Token Endpoint. 
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.TokenEndPointAuthMethod)]
        public string TokenEndPointAuthMethod { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 response_type values
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.ResponseTypes)]
        public List<string> ResponseTypes { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 grant types
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.GrantTypes)]
        public List<string> GrantTypes { get; set; }

        /// <summary>
        /// Gets or sets a list of OAUTH2.0 grant_types.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.AllowedScopes)]
        public List<string> AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets an array of Redirection URI values used by the client.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.RedirectUris)]
        public List<string> RedirectUris { get; set; }

        /// <summary>
        /// Gets or sets the type of application
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.ApplicationType)]
        public string ApplicationType { get; set; }

        /// <summary>
        /// Url for the Client's JSON Web Key Set document
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.JwksUri)]
        public string JwksUri { get; set; }

        /// <summary>
        /// Client json web keys are passed by values
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.Jwks)]
        public JsonWebKeySet Jwks { get; set; }

        /// <summary>
        /// Gets or sets the list of json web keys
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.JsonWebKeys)]
        public List<JsonWebKey> JsonWebKeys { get; set; }

        /// <summary>
        /// Gets or sets the list of contacts
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.Contacts)]
        public List<string> Contacts { get; set; }

        /// <summary>
        /// Get or set the sector identifier uri
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.SectorIdentifierUri)]
        public string SectorIdentifierUri { get; set; }

        /// <summary>
        /// Gets or sets the subject type
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.SubjectType)]
        public string SubjectType { get; set; }

        /// <summary>
        /// Gets or sets the user info signed response algorithm
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.UserInfoSignedResponseAlg)]
        public string UserInfoSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response algorithm
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.UserInfoEncryptedResponseAlg)]
        public string UserInfoEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response enc
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.UserInfoEncryptedResponseEnc)]
        public string UserInfoEncryptedResponseEnc { get; set; }

        /// <summary>
        /// Gets or sets the request objects signing algorithm
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.RequestObjectSigningAlg)]
        public string RequestObjectSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption algorithm
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.RequestObjectEncryptionAlg)]
        public string RequestObjectEncryptionAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption enc
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.RequestObjectEncryptionEnc)]
        public string RequestObjectEncryptionEnc { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint authentication signing algorithm
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.TokenEndPointAuthSigningAlg)]
        public string TokenEndPointAuthSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the default max age
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.DefaultMaxAge)]
        public double DefaultMaxAge { get; set; }

        /// <summary>
        /// Gets or sets the require authentication time
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.RequireAuthTime)]
        public bool RequireAuthTime { get; set; }

        /// <summary>
        /// Gets or sets the default acr values
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.DefaultAcrValues)]
        public string DefaultAcrValues { get; set; }

        /// <summary>
        /// Gets or sets the initiate login uri
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.InitiateLoginUri)]
        public string InitiateLoginUri { get; set; }

        /// <summary>
        /// Gets or sets the list of request uris
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.RequestUris)]
        public List<string> RequestUris { get; set; }

        /// <summary>
        /// Gets or sets the post logout redirection urls.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.PostLogoutRedirectUris)]
        public List<string> PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// Gets or sets the create datetime.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.CreateDateTime)]
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Gets or sets the update datetime.
        /// </summary>
        [DataMember(Name = SharedConstants.ClientNames.UpdateDateTime)]
        public DateTime UpdateDateTime { get; set; }
    }
}
