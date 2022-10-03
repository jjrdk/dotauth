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

namespace DotAuth.Shared;

using System;

/// <summary>
/// Defines the abstract event type.
/// </summary>
public abstract record Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Event"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="timestamp">The timestamp.</param>
    protected Event(string id, DateTimeOffset timestamp)
    {
        Id = id;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Identity the event
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the timestamp.
    /// </summary>
    /// <value>
    /// The timestamp.
    /// </value>
    public DateTimeOffset Timestamp { get; }
}