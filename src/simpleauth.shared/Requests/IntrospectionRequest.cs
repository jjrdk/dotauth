// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the introspection request.
    /// </summary>
    [DataContract]
    public record IntrospectionRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [DataMember(Name = "token")]
#pragma warning disable IDE1006 // Naming Styles
        public string? token { get; set; }

        /// <summary>
        /// Gets or sets the token type hint.
        /// </summary>
        /// <value>
        /// The token type hint.
        /// </value>
        [DataMember(Name = "token_type_hint")]
        public string? token_type_hint { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [DataMember(Name = "client_id")]
        public string? client_id { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        [DataMember(Name = "client_secret")]
        public string? client_secret { get; set; }

        /// <summary>
        /// Gets or sets the client assertion.
        /// </summary>
        /// <value>
        /// The client assertion.
        /// </value>
        [DataMember(Name = "client_assertion")]
        public string? client_assertion { get; set; }

        /// <summary>
        /// Gets or sets the type of the client assertion.
        /// </summary>
        /// <value>
        /// The type of the client assertion.
        /// </value>
        [DataMember(Name = "client_assertion_type")]
        public string? client_assertion_type { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
