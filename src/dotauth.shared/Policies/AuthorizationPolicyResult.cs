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

namespace DotAuth.Shared.Policies;

using System.Collections.Generic;
using System.Security.Claims;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the authorization policy result.
/// </summary>
public sealed class AuthorizationPolicyResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationPolicyResult"/> class.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="principal">The requesting principal.</param>
    /// <param name="errorDetails"></param>
    public AuthorizationPolicyResult(AuthorizationPolicyResultKind result, IReadOnlyList<Claim> principal, object? errorDetails = null)
    {
        Result = result;
        Principal = principal;
        ErrorDetails = errorDetails;
    }

    /// <summary>
    /// Gets the result kind.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public AuthorizationPolicyResultKind Result { get; }

    /// <summary>
    /// Gets the requesting principal.
    /// </summary>
    public IReadOnlyList<Claim> Principal { get; }

    /// <summary>
    /// Get the error details.
    /// </summary>
    /// <value>
    /// The error details.
    /// </value>
    public object? ErrorDetails { get; }
}