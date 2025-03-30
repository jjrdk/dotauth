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

namespace DotAuth.Shared.Models;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the resource set request.
/// </summary>
public record ResourceSet : ResourceSetDescription
{
    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    [JsonPropertyName("resource_scopes")]
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the authorization policies for the resource.
    /// </summary>
    [JsonPropertyName("authorization_policies")]
    public PolicyRule[] AuthorizationPolicies { get; set; } = [];

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
}
