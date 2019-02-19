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
    public class IntrospectionRequest
    {
        [DataMember(Name = "token")]
        public string token { get; set; }
        [DataMember(Name = "token_type_hint")]
        public string token_type_hint { get; set; }
        [DataMember(Name = "client_id")]
        public string client_id { get; set; }
        [DataMember(Name = "client_secret")]
        public string client_secret { get; set; }
        [DataMember(Name = "client_assertion")]
        public string client_assertion { get; set; }
        [DataMember(Name = "client_assertion_type")]
        public string client_assertion_type { get; set; }
    }
}
