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

/// <summary>
/// Defines the resource set request.
/// </summary>
[DataContract]
public record ResourceSet : ResourceSetDescription
{
    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    [DataMember(Name = "resource_scopes")]
    public string[] Scopes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the authorization policies for the resource.
    /// </summary>
    [DataMember(Name = "authorization_policies")]
    public PolicyRule[] AuthorizationPolicies { get; set; } = Array.Empty<PolicyRule>();

    /// <summary>
    /// Gets or sets the metadata for the resource set.
    /// </summary>
    [DataMember(Name = "metadata")]
    public List<KeyValuePair<string, string>> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    [DataMember(Name = "created")]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
}
