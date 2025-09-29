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

using System.Text.Json.Serialization;

/// <summary>
/// Defines the update policy rule.
/// </summary>
public sealed record PolicyRule
{
    /// <summary>
    /// Gets or sets the client ids allowed.
    /// </summary>
    /// <value>
    /// The client ids allowed.
    /// </value>
    [JsonPropertyName("clients")]
    public string[] ClientIdsAllowed { get; set; } = [];

    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    [JsonPropertyName("claims")]
    public ClaimData[] Claims { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this instance is resource owner consent needed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("consent_needed")]
    public bool IsResourceOwnerConsentNeeded { get; set; }

    /// <summary>
    /// Gets or sets the script.
    /// </summary>
    /// <value>
    /// The script.
    /// </value>
    [JsonPropertyName("script")]
    public string? Script { get; set; }

    /// <summary>
    /// Gets or sets the open identifier provider.
    /// </summary>
    /// <value>
    /// The open identifier provider.
    /// </value>
    [JsonPropertyName("provider")]
    public string? OpenIdProvider { get; set; }
}
