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
    /// <seealso cref="Event" />
    public class TokenGranted : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenGranted"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="subject">The subject of the token.</param>
        /// <param name="clientId">The authorized client.</param>
        /// <param name="scopes">The granted scopes</param>
        /// <param name="grantType">The used grant type.</param>
        /// <param name="timestamp">The timestamp.</param>
        public TokenGranted(string id, string? subject, string clientId, string scopes, string grantType, DateTimeOffset timestamp)
        : base(id, timestamp)
        {
            Subject = subject;
            ClientId = clientId;
            Scopes = scopes.Split(' ', ',');
            GrantType = grantType;
        }

        /// <summary>
        /// The subject of the token.
        /// </summary>
        public string? Subject { get; }

        /// <summary>
        /// The authorized client.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// The granted scopes.
        /// </summary>
        public string[] Scopes { get; }

        /// <summary>
        /// The grant type when issuing the token.
        /// </summary>
        public string GrantType { get; }
    }
}
