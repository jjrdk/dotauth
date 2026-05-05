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

using System;
using DotAuth.Shared;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Resolves the current tenant identifier from the HTTP request's host header
/// by extracting the leftmost subdomain label and comparing it against the
/// configured base domain.
/// </summary>
/// <remarks>
/// Resolution algorithm:
/// <list type="number">
///   <item>If no HTTP context is available, return <see cref="_defaultTenantId"/>.</item>
///   <item>If <see cref="_baseDomain"/> is configured and the host exactly matches
///         it, return <see cref="_defaultTenantId"/>.</item>
///   <item>If <see cref="_baseDomain"/> is configured and the host ends with
///         <c>.{baseDomain}</c>, extract the leftmost label as the tenant ID.</item>
///   <item>Otherwise return <see cref="_defaultTenantId"/>.</item>
/// </list>
/// Registered as Singleton — safe because <see cref="IHttpContextAccessor"/>
/// uses <c>AsyncLocal</c> internally and reflects the current request context.
/// </remarks>
internal sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _defaultTenantId;
    private readonly string? _baseDomain;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpTenantContext"/>.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="defaultTenantId">
    /// The fallback tenant identifier used when no tenant can be parsed from the host.
    /// </param>
    /// <param name="baseDomain">
    /// The authoritative base domain (e.g., <c>"auth.example.com"</c>) used to
    /// detect subdomain labels. Pass <c>null</c> to always use the default tenant.
    /// </param>
    public HttpTenantContext(
        IHttpContextAccessor httpContextAccessor,
        string defaultTenantId,
        string? baseDomain)
    {
        _httpContextAccessor = httpContextAccessor;
        _defaultTenantId = defaultTenantId;
        _baseDomain = baseDomain?.ToLowerInvariant();
    }

    /// <inheritdoc />
    public string TenantId
    {
        get
        {
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
            if (string.IsNullOrEmpty(host))
            {
                return _defaultTenantId;
            }

            if (string.IsNullOrEmpty(_baseDomain))
            {
                // No base domain configured — always use default tenant.
                return _defaultTenantId;
            }

            var lowerHost = host.ToLowerInvariant();

            // Direct hit to the base domain itself means the default tenant.
            if (string.Equals(lowerHost, _baseDomain, StringComparison.Ordinal))
            {
                return _defaultTenantId;
            }

            // Subdomain must be exactly one label deep (no nested subdomains).
            var suffix = "." + _baseDomain;
            if (lowerHost.EndsWith(suffix, StringComparison.Ordinal))
            {
                var label = lowerHost[..^suffix.Length];

                // Reject multi-level subdomains like "a.b.auth.example.com".
                if (!label.Contains('.') && label.Length > 0)
                {
                    return label;
                }
            }

            return _defaultTenantId;
        }
    }
}


