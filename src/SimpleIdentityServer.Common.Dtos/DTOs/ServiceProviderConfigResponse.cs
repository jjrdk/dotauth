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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class ServiceProviderConfigResponse : ScimResource
    {
        /// <summary>
        /// An HTTP-addressable URL pointing to the service provider's human-consumable help documentation.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.DocumentationUri)]
        public string DocumentationUri { get; set; }

        /// <summary>
        /// Specifies PATCH configuration options.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.Patch)]
        public PatchResponse Patch { get; set; }

        /// <summary>
        /// Specifies bulk configuration options.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.Bulk)]
        public BulkResponse Bulk { get; set; }

        /// <summary>
        /// Specifies FILTER options.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.Filter)]
        public FilterResponse Filter { get; set; }

        /// <summary>
        /// Configuration options related to changing a password.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.ChangePassword)]
        public ChangePasswordResponse ChangePassword { get; set; }

        /// <summary>
        /// Sort configuration options.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.Sort)]
        public SortResponse Sort { get; set; }

        /// <summary>
        /// ETag configuration options.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.Etag)]
        public EtagResponse Etag { get; set; }

        /// <summary>
        /// Supported authentication scheme properties.
        /// </summary>
        [DataMember(Name = ScimConstants.ServiceProviderConfigResponseNames.AuthenticationSchemes)]
        public IEnumerable<AuthenticationSchemeResponse> AuthenticationSchemes { get; set; }
    }
}
