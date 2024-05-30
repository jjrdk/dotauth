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

/// <summary>
/// Defines the consent content.
/// </summary>
public sealed record Consent
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Gets or sets the client id.
    /// </summary>
    /// <value>
    /// The client id.
    /// </value>
    public string ClientId { get; init; } = null!;

    /// <summary>
    /// Gets or sets the client name.
    /// </summary>
    /// <value>
    /// The client name.
    /// </value>
    public string ClientName { get; init; } = null!;

    /// <summary>
    /// Gets or sets the policy <see cref="Uri"/>.
    /// </summary>
    public Uri? PolicyUri { get; init; }

    /// <summary>
    /// Gets or sets the terms of service <see cref="Uri"/>.
    /// </summary>
    public Uri? TosUri { get; init; }

    /// <summary>
    /// Gets or sets the resource owner's subject.
    /// </summary>
    /// <value>
    /// The resource owner's subject.
    /// </value>
    public string Subject { get; init; } = null!;

    /// <summary>
    /// Gets or sets the granted scopes.
    /// </summary>
    /// <value>
    /// The granted scopes.
    /// </value>
    public string[] GrantedScopes { get; init; } = [];

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    public string[] Claims { get; init; } = [];
}
