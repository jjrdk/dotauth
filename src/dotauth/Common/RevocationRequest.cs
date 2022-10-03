﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Common;

/// <summary>
/// Defines the revocation request.
/// </summary>
public sealed record RevocationRequest
{
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    /// <value>
    /// The token.
    /// </value>
    public string? token { get; init; }

    /// <summary>
    /// Gets or sets the token type hint.
    /// </summary>
    /// <value>
    /// The token type hint.
    /// </value>
    public string? token_type_hint { get; init; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public string? client_id { get; init; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    /// <value>
    /// The client secret.
    /// </value>
    public string? client_secret { get; init; }

    /// <summary>
    /// Gets or sets the type of the client assertion.
    /// </summary>
    /// <value>
    /// The type of the client assertion.
    /// </value>
    public string? client_assertion_type { get; init; }

    /// <summary>
    /// Gets or sets the client assertion.
    /// </summary>
    /// <value>
    /// The client assertion.
    /// </value>
    public string? client_assertion { get; init; }
#pragma warning restore IDE1006 // Naming Styles
}