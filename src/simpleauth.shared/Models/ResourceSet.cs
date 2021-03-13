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

namespace SimpleAuth.Shared.Models
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the update resource set request.
    /// </summary>
    [DataContract]
    public record ResourceSet
    {
        /// <summary>
        /// Gets or sets the id of the resource set.
        /// </summary>
        [DataMember(Name = "_id")]
        public string Id { get; init; } = null!;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "name")]
        public string? Name { get; init; }

        /// <summary>
        /// Gets or sets the resource description.
        /// </summary>
        [DataMember(Name = "description")]
        public string? Description { get; init; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMember(Name = "type")]
        public string? Type { get; init; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        [DataMember(Name = "resource_scopes")]
        public string[] Scopes { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the icon URI.
        /// </summary>
        /// <value>
        /// The icon URI.
        /// </value>
        [DataMember(Name = "icon_uri")]
        public Uri? IconUri { get; init; }

        /// <summary>
        /// Gets or sets the authorization policies for the resource.
        /// </summary>
        [DataMember(Name = "authorization_policies")]
        public PolicyRule[] AuthorizationPolicies { get; init; } = Array.Empty<PolicyRule>();
    }
}
