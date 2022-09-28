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

namespace SimpleAuth.Shared.Events.Openid;

using System;
using System.Linq;

/// <summary>
/// Defines the consent accepted event.
/// </summary>
/// <seealso cref="SimpleAuth.Shared.Event" />
public sealed record ConsentAccepted : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentAccepted"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="scope">The accepted scope.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="subject">The accepting subject.</param>
    /// <param name="clientId">The accepted client.</param>
    public ConsentAccepted(string id, string subject, string clientId, string scope, DateTimeOffset timestamp)
        : this(id, subject, clientId, scope.Split(' ', ','), timestamp)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentAccepted"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="scope">The accepted scope.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="subject">The accepting subject.</param>
    /// <param name="clientId">The accepted client.</param>
    public ConsentAccepted(string id, string subject, string clientId, string[] scope, DateTimeOffset timestamp)
        : base(id, timestamp)
    {
        Subject = subject;
        ClientId = clientId;
        Scope = scope.ToArray();
    }

    /// <summary>
    /// The accepting subject.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// The consented client.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// The consented scope.
    /// </summary>
    public string[] Scope { get; }
}