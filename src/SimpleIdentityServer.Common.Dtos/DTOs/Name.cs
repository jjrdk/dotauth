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

namespace SimpleIdentityServer.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Name
    {
        [DataMember(Name = ScimConstants.NameResponseNames.Formatted)]
        public string Formatted { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.FamilyName)]
        public string FamilyName { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.GivenName)]
        public string GivenName { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.MiddleName)]
        public string MiddleName { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.HonorificPrefix)]
        public string HonorificPrefix { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.HonorificSuffix)]
        public string HonorificSuffix { get; set; }
    }
}