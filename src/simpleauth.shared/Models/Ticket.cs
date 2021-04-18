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

    /// <summary>
    /// Defines the ticket content.
    /// </summary>
    public record Ticket
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; init; } = null!;

        /// <summary>
        /// Gets or sets the owner of the resource that the ticket relates to.
        /// </summary>
        public string ResourceOwner { get; init; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is authorized by ro.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authorized by ro; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthorizedByRo { get; init; }

        /// <summary>
        /// Gets or sets the expiration date time.
        /// </summary>
        /// <value>
        /// The expiration date time.
        /// </value>
        public DateTimeOffset Expires { get; init; }

        /// <summary>
        /// Gets or sets the create date time.
        /// </summary>
        /// <value>
        /// The create date time.
        /// </value>
        public DateTimeOffset Created { get; init; }

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>
        /// The lines.
        /// </value>
        public TicketLine[] Lines { get; init; } = Array.Empty<TicketLine>();

        /// <summary>
        /// Gets or sets the claims associated with the requester.
        /// </summary>
        public ClaimData[] Requester { get; init; } = Array.Empty<ClaimData>();
    }
}
