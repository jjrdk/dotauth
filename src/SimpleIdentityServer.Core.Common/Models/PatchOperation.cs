// Copyright 2015 Habart Thierry
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

namespace SimpleIdentityServer.Core.Common.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PatchOperation
    {
        [JsonProperty("op")]
        public PatchOperations Type { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("value")]
        public JToken Value { get; set; }
    }

    public class PatchRequest
    {
        public PatchRequest()
        {
            Schemas = new[] {ScimConstants.Messages.PatchOp};
        }

        public string[] Schemas { get; }

        public PatchOperation[] Operations { get; set; }
    }
}
