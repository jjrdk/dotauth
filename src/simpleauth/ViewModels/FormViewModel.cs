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
    /// <summary>
    /// Defines the form view model.
    /// </summary>
    public class FormViewModel
    {
        /// <summary>
        /// Gets or sets the id token.
        /// </summary>
        public string? IdToken { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the redirect uri.
        /// </summary>
        public string? RedirectUri { get; set; }
    }
}