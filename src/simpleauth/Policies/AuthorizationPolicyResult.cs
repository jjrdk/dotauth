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

namespace SimpleAuth.Policies
{
    using Shared.Responses;

    /// <summary>
    /// Defines the authorization policy result.
    /// </summary>
    internal class AuthorizationPolicyResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationPolicyResult"/> class.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="errorDetails"></param>
        public AuthorizationPolicyResult(AuthorizationPolicyResultKind result, object errorDetails = null)
        {
            Result = result;
            ErrorDetails = errorDetails;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public AuthorizationPolicyResultKind Result { get; }

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        /// <value>
        /// The error details.
        /// </value>
        public object ErrorDetails { get; }
    }
}
