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

namespace DotAuth.Shared.Events.Openid;

using System;

/// <summary>
/// Defines the resource owner authenticated event
/// </summary>
/// <seealso cref="Event" />
public sealed record ResourceOwnerAuthenticated : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceOwnerAuthenticated"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="resourceOwnerId"></param>
    /// <param name="amr"></param>
    /// <param name="timestamp">The timestamp.</param>
    public ResourceOwnerAuthenticated(string id, string resourceOwnerId, string? amr, DateTimeOffset timestamp)
        : base(id, timestamp)
    {
        ResourceOwnerId = resourceOwnerId;
        Amr = amr ?? string.Empty;
    }

    /// <summary>
    /// Gets the resource owner identifier.
    /// </summary>
    /// <value>
    /// The resource owner identifier.
    /// </value>
    public string ResourceOwnerId { get; }

    /// <summary>
    /// Gets the amr.
    /// </summary>
    /// <value>
    /// The amr.
    /// </value>
    public string Amr { get; }
}