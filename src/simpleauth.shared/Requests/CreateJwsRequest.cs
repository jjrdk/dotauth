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

    [DataContract]
    public class CreateJwsRequest
    {
        /// <summary>
        /// Gets or sets the JSON WEB KEY KID
        /// </summary>
        [DataMember(Name = SharedConstants.CreateJwsRequestNames.Kid)]
        public string Kid { get; set; }

        /// <summary>
        /// Gets or sets the sign alg
        /// </summary>
        [DataMember(Name = SharedConstants.CreateJwsRequestNames.Alg)]
        public JwsAlg Alg { get; set; }

        /// <summary>
        /// Gets or sets the JWKS URL
        /// </summary>
        [DataMember(Name = SharedConstants.CreateJwsRequestNames.Url)]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the Payload
        /// </summary>
        [DataMember(Name = SharedConstants.CreateJwsRequestNames.Payload)]
        public JwsPayload Payload { get; set; }
    }
}
