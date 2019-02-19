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

namespace SimpleAuth.Shared.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the resource owner response.
    /// </summary>
    [DataContract]
    public class ResourceOwnerResponse
    {
        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        [DataMember(Name = "login")]
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [DataMember(Name = "password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is local account.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is local account; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "is_localaccount")]
        public bool IsLocalAccount { get; set; }

        /// <summary>
        /// Gets or sets the two factor authentication.
        /// </summary>
        /// <value>
        /// The two factor authentication.
        /// </value>
        [DataMember(Name = "two_factor_auth")]
        public string TwoFactorAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public List<KeyValuePair<string, string>> Claims { get; set; }

        /// <summary>
        /// Gets or sets the create date time.
        /// </summary>
        /// <value>
        /// The create date time.
        /// </value>
        [DataMember(Name = "create_datetime")]
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Gets or sets the update date time.
        /// </summary>
        /// <value>
        /// The update date time.
        /// </value>
        [DataMember(Name = "update_datetime")]
        public DateTime UpdateDateTime { get; set; }
    }
}
