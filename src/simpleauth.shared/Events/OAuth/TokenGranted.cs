// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Shared.Events.OAuth
{
    using System;

    /// <summary>
    /// Defines the toke granted event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class TokenGranted : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenGranted"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="timestamp">The timestamp.</param>
        public TokenGranted(string id, string processId, string accessToken, DateTime timestamp)
        :base(id, timestamp)
        {
            ProcessId = processId;
            AccessToken = accessToken;
        }

        /// <summary>
        /// Gets the process identifier.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        public string ProcessId { get; }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string AccessToken { get; }
    }
}
