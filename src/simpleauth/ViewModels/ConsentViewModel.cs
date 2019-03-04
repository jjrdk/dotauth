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

namespace SimpleAuth.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the consent view model.
    /// </summary>
    public class ConsentViewModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the client.
        /// </summary>
        /// <value>
        /// The display name of the client.
        /// </value>
        public string ClientDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the allowed scope descriptions.
        /// </summary>
        /// <value>
        /// The allowed scope descriptions.
        /// </value>
        public ICollection<string> AllowedScopeDescriptions { get; set; }

        /// <summary>
        /// Gets or sets the allowed individual claims.
        /// </summary>
        /// <value>
        /// The allowed individual claims.
        /// </value>
        public ICollection<string> AllowedIndividualClaims { get; set; }

        /// <summary>
        /// Gets or sets the logo URI.
        /// </summary>
        /// <value>
        /// The logo URI.
        /// </value>
        public string LogoUri { get; set; }

        /// <summary>
        /// Gets or sets the policy URI.
        /// </summary>
        /// <value>
        /// The policy URI.
        /// </value>
        public string PolicyUri { get; set; }

        /// <summary>
        /// Gets or sets the Terms of Service URI.
        /// </summary>
        /// <value>
        /// The tos URI.
        /// </value>
        public string TosUri { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; set; }
    }
}
