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

namespace SimpleAuth.Shared.Events.Openid
{
    using System;

    /// <summary>
    /// Defines the consent rejected event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class ConsentRejected : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentRejected"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="scopes">The rejected scopes.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="clientId">The rejected client.</param>
        public ConsentRejected(string id, string clientId, string[] scopes, DateTimeOffset timestamp)
            : base(id, timestamp)
        {
            ClientId = clientId;
            Scopes = scopes;
        }

        /// <summary>
        /// Get the rejected client id.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets the rejected scopes.
        /// </summary>
        public string[] Scopes { get; }
    }
}
