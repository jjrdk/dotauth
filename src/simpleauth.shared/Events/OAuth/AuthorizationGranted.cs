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
    /// Defines the authorization granted event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class AuthorizationGranted : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationGranted"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="timestamp">The timestamp.</param>
        public AuthorizationGranted(string id, string processId, object payload, DateTime timestamp)
        : base(id, timestamp)
        {
            ProcessId = processId;
            Payload = payload;
        }

        /// <summary>
        /// Gets the process identifier.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        public string ProcessId { get; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        public object Payload { get; }
    }
}
