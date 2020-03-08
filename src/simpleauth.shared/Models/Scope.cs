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
    /// Defines the scope.
    /// </summary>
    [DataContract]
    public class Scope
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is displayed in consent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is displayed in consent; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "is_displayed_in_consent")]
        public bool IsDisplayedInConsent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is open identifier scope.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open identifier scope; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "is_openid_scope")]
        public bool IsOpenIdScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is exposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is exposed; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "is_exposed")]
        public bool IsExposed { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public string[] Claims { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the create datetime.
        /// </summary>
        [DataMember(Name = "create_datetime")]
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Gets or sets the update datetime.
        /// </summary>
        [DataMember(Name = "update_datetime")]
        public DateTime UpdateDateTime { get; set; }
    }
}
