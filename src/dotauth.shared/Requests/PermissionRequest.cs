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

namespace DotAuth.Shared.Requests;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the add permission request.
/// </summary>
public sealed record PermissionRequest
{
    /// <summary>
    /// Gets or sets the resource set identifier.
    /// </summary>
    /// <value>
    /// The resource set identifier.
    /// </value>
    [JsonPropertyName("resource_set_id")]
    public string? ResourceSetId { get; set; }

    /// <summary>
    /// Gets or sets the requested scopes.
    /// </summary>
    [JsonPropertyName("scopes")]
    public string[]? Scopes { get; set; }

    /// <summary>
    /// Gets or sets the id token of the ticket requester.
    /// </summary>
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}
