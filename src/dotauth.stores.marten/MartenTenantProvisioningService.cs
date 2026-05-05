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

namespace DotAuth.Stores.Marten;

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Stores.Marten.Containers;
using global::Marten;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Marten-backed implementation of <see cref="ITenantProvisioningService"/> that
/// provisions a tenant by:
/// <list type="bullet">
///   <item>Generating an RSA-2048 signing key pair (private + public) stored as
///         <see cref="JsonWebKeyContainer"/> documents scoped to the tenant.</item>
///   <item>Seeding the built-in OIDC standard scopes plus any caller-supplied
///         additional scopes, skipping those that already exist.</item>
/// </list>
/// All operations open a tenant-scoped Marten session directly, bypassing the
/// HTTP-context-based <c>ITenantContext</c>, so provisioning can run safely from
/// hosted services and background jobs with no active HTTP request.
/// </summary>
public sealed class MartenTenantProvisioningService : ITenantProvisioningService
{
    /// <summary>
    /// Standard OIDC scopes seeded for every new tenant.
    /// </summary>
    private static readonly Scope[] StandardOidcScopes =
    [
        new Scope
        {
            Name = "openid",
            Description = "OpenID Connect scope",
            Type = ScopeTypes.ResourceOwner,
            IsExposed = true,
            IsDisplayedInConsent = false,
            Claims = [OpenIdClaimTypes.Subject]
        },
        new Scope
        {
            Name = "profile",
            Description = "User profile information",
            Type = ScopeTypes.ResourceOwner,
            IsExposed = true,
            IsDisplayedInConsent = true,
            Claims =
            [
                OpenIdClaimTypes.Name,
                OpenIdClaimTypes.FamilyName,
                OpenIdClaimTypes.GivenName,
                OpenIdClaimTypes.MiddleName,
                OpenIdClaimTypes.NickName,
                OpenIdClaimTypes.PreferredUserName,
                OpenIdClaimTypes.Profile,
                OpenIdClaimTypes.Picture,
                OpenIdClaimTypes.WebSite,
                OpenIdClaimTypes.Gender,
                OpenIdClaimTypes.BirthDate,
                OpenIdClaimTypes.ZoneInfo,
                OpenIdClaimTypes.Locale,
                OpenIdClaimTypes.UpdatedAt
            ]
        },
        new Scope
        {
            Name = "email",
            Description = "User email address",
            Type = ScopeTypes.ResourceOwner,
            IsExposed = true,
            IsDisplayedInConsent = true,
            Claims = [OpenIdClaimTypes.Email, OpenIdClaimTypes.EmailVerified]
        },
        new Scope
        {
            Name = "address",
            Description = "User postal address",
            Type = ScopeTypes.ResourceOwner,
            IsExposed = true,
            IsDisplayedInConsent = true,
            Claims = [OpenIdClaimTypes.Address]
        },
        new Scope
        {
            Name = "phone",
            Description = "User phone number",
            Type = ScopeTypes.ResourceOwner,
            IsExposed = true,
            IsDisplayedInConsent = true,
            Claims = [OpenIdClaimTypes.PhoneNumber, OpenIdClaimTypes.PhoneNumberVerified]
        },
        new Scope
        {
            Name = "role",
            Description = "User roles",
            Type = ScopeTypes.ResourceOwner,
            IsExposed = true,
            IsDisplayedInConsent = true,
            Claims = [OpenIdClaimTypes.Role]
        }
    ];

    private readonly IDocumentStore _documentStore;
    private readonly ILogger<MartenTenantProvisioningService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MartenTenantProvisioningService"/>.
    /// </summary>
    /// <param name="documentStore">The Marten document store.</param>
    /// <param name="logger">The logger.</param>
    public MartenTenantProvisioningService(
        IDocumentStore documentStore,
        ILogger<MartenTenantProvisioningService> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> IsProvisionedAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        // Open a read-only query session scoped to the target tenant.
        await using var session = _documentStore.QuerySession(tenantId);
        return await session.Query<JsonWebKeyContainer>()
            .AnyAsync(x => x.Jwk.HasPrivateKey == true, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ProvisionAsync(
        string tenantId,
        Scope[]? additionalScopes = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        _logger.LogDebug("Provisioning tenant '{TenantId}'", tenantId);

        // Open a write session scoped directly to this tenant, bypassing
        // the HTTP-context ITenantContext so provisioning works from any context.
        await using var session = _documentStore.LightweightSession(tenantId);

        await EnsureSigningKeyAsync(session, tenantId, cancellationToken).ConfigureAwait(false);
        await EnsureScopesAsync(session, additionalScopes, cancellationToken).ConfigureAwait(false);

        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Tenant '{TenantId}' provisioned", tenantId);
        return true;
    }

    /// <summary>
    /// Generates and stores an RSA-2048 signing key pair for the tenant if none
    /// exists yet. Both the private and public JWKs are stored so that signing
    /// and JWKS discovery work out of the box.
    /// </summary>
    private async Task EnsureSigningKeyAsync(
        IDocumentSession session,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var hasKey = await session.Query<JsonWebKeyContainer>()
            .AnyAsync(x => x.Jwk.HasPrivateKey == true, cancellationToken)
            .ConfigureAwait(false);

        if (hasKey)
        {
            _logger.LogDebug("Tenant '{TenantId}' already has a signing key — skipping key generation", tenantId);
            return;
        }

        _logger.LogInformation("Generating RSA-2048 signing key for tenant '{TenantId}'", tenantId);

        // Generate a fresh RSA-2048 key pair.
        using var rsa = RSA.Create(2048);
        var keyId = Guid.CreateVersion7().ToString("N");

        // Private key — stored per-tenant with HasPrivateKey=true.
        // Used internally for signing JWTs.
        var privateRsaKey = new RsaSecurityKey(rsa) { KeyId = keyId };
        var privateJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(privateRsaKey);
        privateJwk.Use = JsonWebKeyUseNames.Sig;
        privateJwk.Alg = SecurityAlgorithms.RsaSha256;

        // Public key — exported separately so GetPublicKeys() returns only
        // public material and no private parameters are ever exposed.
        using var publicRsa = RSA.Create();
        publicRsa.ImportParameters(rsa.ExportParameters(includePrivateParameters: false));
        var publicRsaKey = new RsaSecurityKey(publicRsa) { KeyId = keyId };
        var publicJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(publicRsaKey);
        publicJwk.Use = JsonWebKeyUseNames.Sig;
        publicJwk.Alg = SecurityAlgorithms.RsaSha256;

        session.Store(JsonWebKeyContainer.Create(privateJwk));
        session.Store(JsonWebKeyContainer.Create(publicJwk));
    }

    /// <summary>
    /// Seeds the standard OIDC scopes and any <paramref name="additionalScopes"/>
    /// for the tenant, skipping scopes that already exist.
    /// </summary>
    private async Task EnsureScopesAsync(
        IDocumentSession session,
        Scope[]? additionalScopes,
        CancellationToken cancellationToken)
    {
        // Determine which scope names already exist in this tenant's namespace.
        var existing = await session.Query<ScopeContainer>()
            .Select(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingNames = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allScopes = StandardOidcScopes.Concat(additionalScopes ?? []);
        foreach (var scope in allScopes)
        {
            if (existingNames.Contains(scope.Name))
            {
                continue;
            }

            _logger.LogDebug("Seeding scope '{ScopeName}'", scope.Name);
            session.Store(ScopeContainer.Create(scope));
            existingNames.Add(scope.Name); // avoid duplicates within the same batch
        }
    }
}


