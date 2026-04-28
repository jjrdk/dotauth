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

/// <summary>
/// Provides the tenant identifier for the current execution context.
/// </summary>
/// <remarks>
/// Implementations resolve the tenant from the HTTP request (subdomain extraction)
/// and fall back to a configured default when no tenant can be determined.
/// </remarks>
public interface ITenantContext
{
    /// <summary>
    /// Gets the identifier of the current tenant.
    /// Never null or empty — falls back to the configured <c>DefaultTenantId</c>.
    /// </summary>
    string TenantId { get; }
}

