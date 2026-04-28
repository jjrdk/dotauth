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

namespace DotAuth.Shared.Repositories;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;

/// <summary>
/// Provisions a new tenant with the required signing keys and default scopes.
/// Implementations must be idempotent: repeated calls for the same tenant
/// must not create duplicate signing keys or scopes.
/// </summary>
/// <example>
/// <code>
/// // In application startup (executed by TenantProvisioningHostedService):
/// await provisioningService.ProvisionAsync("acme", cancellationToken: ct);
///
/// // With extra application-specific scopes:
/// var apiScope = new Scope { Name = "api.read", Type = ScopeTypes.ProtectedApi, ... };
/// await provisioningService.ProvisionAsync("acme", [apiScope], ct);
/// </code>
/// </example>
public interface ITenantProvisioningService
{
    /// <summary>
    /// Provisions the specified tenant by ensuring a signing key and the standard
    /// OIDC scopes exist. Caller-supplied <paramref name="additionalScopes"/> are
    /// also seeded but will not overwrite existing scopes.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to provision. Must not be null or whitespace.</param>
    /// <param name="additionalScopes">
    /// Optional extra scopes to seed alongside the default OIDC scopes.
    /// Existing scopes with the same name are left untouched.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if provisioning completed successfully; otherwise <c>false</c>.</returns>
    Task<bool> ProvisionAsync(
        string tenantId,
        Scope[]? additionalScopes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> when the specified tenant already has at least one
    /// signing key configured, meaning previous provisioning completed.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<bool> IsProvisionedAsync(string tenantId, CancellationToken cancellationToken = default);
}

