// Copyright © 2018 Jacob Reimers
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
/// An <see cref="ITenantContext"/> implementation that always returns
/// the tenant identifier supplied at construction time.
/// </summary>
/// <remarks>
/// Useful in tests, hosted services, and one-off utilities where
/// the tenant ID is known statically and no HTTP context is available.
/// </remarks>
/// <example>
/// <code>
/// var ctx = new StaticTenantContext("acme");
/// var store = new RedisTokenStore(db, ctx);
/// </code>
/// </example>
public sealed class StaticTenantContext : ITenantContext
{
    /// <summary>
    /// Initializes a new instance with the given fixed <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to return. Must not be null or whitespace.</param>
    public StaticTenantContext(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        TenantId = tenantId;
    }

    /// <inheritdoc />
    public string TenantId { get; }
}


