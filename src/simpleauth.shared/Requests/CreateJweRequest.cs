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

namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the request to create a JWS.
    /// </summary>
    [DataContract]
    public class CreateJweRequest
    {
        /// <summary>
        /// Gets or sets the JWS.
        /// </summary>
        /// <value>
        /// The JWS.
        /// </value>
        [DataMember(Name = "jws")]
        public string Jws { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember(Name = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the kid.
        /// </summary>
        /// <value>
        /// The kid.
        /// </value>
        [DataMember(Name = "kid")]
        public string Kid { get; set; }

        /// <summary>
        /// Gets or sets the alg.
        /// </summary>
        /// <value>
        /// The alg.
        /// </value>
        [DataMember(Name = "alg")]
        public string Alg { get; set; }

        /// <summary>
        /// Gets or sets the enc.
        /// </summary>
        /// <value>
        /// The enc.
        /// </value>
        [DataMember(Name = "enc")]
        public string Enc { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}
