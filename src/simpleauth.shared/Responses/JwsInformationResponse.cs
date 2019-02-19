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
    using System.IdentityModel.Tokens.Jwt;
    using System.Runtime.Serialization;
    using Requests;

    /// <summary>
    /// Defines the JWS information response.
    /// </summary>
    [DataContract]
    public class JwsInformationResponse
    {
        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [DataMember(Name = "header")]
        public JwsProtectedHeader Header { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        [DataMember(Name = "payload")]
        public JwtPayload Payload { get; set; }

        /// <summary>
        /// Gets or sets the json web key.
        /// </summary>
        /// <value>
        /// The json web key.
        /// </value>
        [DataMember(Name = "jsonwebkey")]
        public Dictionary<string, object> JsonWebKey { get; set; }
    }
}
