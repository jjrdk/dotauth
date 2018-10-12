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

namespace SimpleIdentityServer.Core.Common.DTOs
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BulkResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkResponseNames.Supported)]
        public bool Supported { get; set; }

        /// <summary>
        /// Maximum number of operations.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkResponseNames.MaxOperations)]
        public int MaxOperations { get; set; }

        /// <summary>
        /// Maximum payload size in bytes.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkResponseNames.MaxPayloadSize)]
        public int MaxPayloadSize { get; set; }
    }

    [DataContract]
    public sealed class PatchResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.PatchResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    [DataContract]
    public sealed class FilterResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.FilterResponseNames.Supported)]
        public bool Supported { get; set; }

        /// <summary>
        /// Maximum number of resources returned in the response.
        /// </summary>
        [DataMember(Name = ScimConstants.FilterResponseNames.MaxResults)]
        public int MaxResults { get; set; }
    }

    [DataContract]
    public sealed class ChangePasswordResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.ChangePasswordResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    [DataContract]
    public sealed class SortResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.SortResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    [DataContract]
    public sealed class EtagResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.EtagResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    public enum AuthenticationTypes
    {
        oauth,
        oauth2,
        oauthbearertoken,
        httpbasic,
        httpdigest
    }

    [DataContract]
    public sealed class AuthenticationSchemeResponse : MultiValueAttr
    {
        /// <summary>
        /// Authentication scheme.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.Type)]
        public AuthenticationTypes AuthenticationType { get; set; }

        /// <summary>
        /// Common authentication scheme name.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.Name)]
        public string Name { get; set; }

        /// <summary>
        /// A description of the authentication scheme.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.Description)]
        public string Description { get; set; }

        /// <summary>
        /// An HTTP-Addressable URL pointing to the authentication schem's specification.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.SpecUri)]
        public string SpecUri { get; set; }

        /// <summary>
        /// An HTTP-Addressable URL pointing to the authentication scheme's usage documentation.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.DocumentationUri)]
        public string DocumentationUri { get; set; }
    }

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
