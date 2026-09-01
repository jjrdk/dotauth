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

namespace DotAuth.Services;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;

/// <summary>
/// A no-op implementation of <see cref="ITenantProvisioningService"/> used as
/// the default fallback when no concrete provisioning service (such as the
/// Marten-backed implementation) has been registered.
/// </summary>
/// <remarks>
/// Provisioning steps (key generation, scope seeding) are skipped entirely.
/// Replace this with <c>MartenTenantProvisioningService</c> (from
/// <c>dotauth.stores.marten</c>) in production deployments.
/// </remarks>
internal sealed class NullTenantProvisioningService : ITenantProvisioningService
{
    /// <inheritdoc />
    /// <remarks>Always returns <c>true</c> without performing any work.</remarks>
    public Task<bool> ProvisionAsync(
        string tenantId,
        Scope[]? additionalScopes = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    /// <remarks>Always returns <c>true</c> (assumes tenant is provisioned).</remarks>
    public Task<bool> IsProvisionedAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

