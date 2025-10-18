﻿namespace DotAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;

/// <summary>
/// Defines the in-memory scope repository.
/// </summary>
/// <seealso cref="IScopeRepository" />
internal sealed class InMemoryScopeRepository : IScopeRepository
{
    private readonly ICollection<Scope> _scopes;

    private readonly List<Scope> _defaultScopes =
    [
        new Scope
        {
            Name = "offline",
            IsExposed = true,
            IsDisplayedInConsent = true,
            Description = Strings.AccessToRefreshToken,
            Type = ScopeTypes.ProtectedApi,
            Claims = []
        },

        new Scope
        {
            Name = "openid",
            IsExposed = true,
            IsDisplayedInConsent = true,
            Description = Strings.AccessToOpenIdScope,
            Type = ScopeTypes.ResourceOwner,
            Claims = []
        },

        new Scope
        {
            Name = "profile",
            IsExposed = true,
            Description = Strings.AccessToProfileInformation,
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
            ],
            Type = ScopeTypes.ResourceOwner,
            IsDisplayedInConsent = true
        },

        new Scope
        {
            Name = "email",
            IsExposed = true,
            IsDisplayedInConsent = true,
            Description = Strings.AccessToEmailAddresses,
            Claims = [OpenIdClaimTypes.Email, OpenIdClaimTypes.EmailVerified],
            Type = ScopeTypes.ResourceOwner
        },

        new Scope
        {
            Name = "address",
            IsExposed = true,
            IsDisplayedInConsent = true,
            Description = Strings.AccessToAddressInformation,
            Claims = [OpenIdClaimTypes.Address],
            Type = ScopeTypes.ResourceOwner
        },

        new Scope
        {
            Name = "phone",
            IsExposed = true,
            IsDisplayedInConsent = true,
            Description = Strings.AccessToPhoneInformation,
            Claims = [OpenIdClaimTypes.PhoneNumber, OpenIdClaimTypes.PhoneNumberVerified],
            Type = ScopeTypes.ResourceOwner
        },

        new Scope
        {
            Name = "role",
            IsExposed = true,
            IsDisplayedInConsent = true,
            Description = Strings.AccessToRoles,
            Claims = [OpenIdClaimTypes.Role],
            Type = ScopeTypes.ResourceOwner
        },

        new Scope
        {
            Claims = [OpenIdClaimTypes.Role],
            Name = "register_client",
            IsExposed = false,
            IsDisplayedInConsent = false,
            Description = Strings.RegisterAClient,
            Type = ScopeTypes.ProtectedApi
        },

        new Scope
        {
            Claims = [OpenIdClaimTypes.Role],
            Description = Strings.ManageServerResources,
            IsDisplayedInConsent = true,
            IsExposed = false,
            Name = "manager",
            Type = ScopeTypes.ProtectedApi
        },

        new Scope
        {
            Claims = [OpenIdClaimTypes.Subject],
            Description = Strings.ManageUma,
            IsDisplayedInConsent = true,
            IsExposed = true,
            Name = "uma_protection",
            Type = ScopeTypes.ProtectedApi
        }
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryScopeRepository"/> class.
    /// </summary>
    /// <param name="scopes">The scopes.</param>
    /// <param name="includeDefaultScopes">Include default scope definitions during instantiation.</param>
    public InMemoryScopeRepository(IReadOnlyCollection<Scope>? scopes = null, bool includeDefaultScopes = true)
    {
        _scopes = scopes == null || scopes.Count == 0
            ? _defaultScopes
            : scopes.Concat(includeDefaultScopes ? _defaultScopes.AsEnumerable() : [])
                .Distinct()
                .ToList();
    }

    /// <inheritdoc />
    public Task<bool> Delete(Scope scope, CancellationToken cancellationToken)
    {
        if (scope == null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        var sc = _scopes.FirstOrDefault(s => s.Name == scope.Name);
        if (sc == null)
        {
            return Task.FromResult(false);
        }

        _scopes.Remove(sc);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<Scope[]> GetAll(CancellationToken cancellationToken = default)
    {
        var res = _scopes.ToArray();
        return Task.FromResult(res);
    }

    /// <inheritdoc />
    public Task<Scope?> Get(string name, CancellationToken cancellationToken = default)
    {
        var scope = _scopes.FirstOrDefault(s => s.Name == name);

        return Task.FromResult(scope);
    }

    /// <inheritdoc />
    public Task<bool> Insert(Scope scope, CancellationToken cancellationToken)
    {
        if (_scopes.Any(x => x.Name == scope.Name))
        {
            return Task.FromResult(false);
        }

        _scopes.Add(scope);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<PagedResult<Scope>> Search(
        SearchScopesRequest parameter,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Scope> result = _scopes;
        if (parameter.ScopeNames != null && parameter.ScopeNames.Any())
        {
            result = result.Where(c => parameter.ScopeNames.Any(n => c.Name.Contains(n)));
        }

        if (parameter.ScopeTypes?.Any() == true)
        {
            var scopeTypes = parameter.ScopeTypes.Select(t => t);
            result = result.Where(s => scopeTypes.Contains(s.Type));
        }

        result = parameter.Descending
            ? result.OrderByDescending(c => c.Name)
            : result.OrderBy(c => c.Name);

        if (parameter.NbResults > 0)
        {
            result = result.Skip(parameter.StartIndex).Take(parameter.NbResults);
        }

        var content = result.ToArray();
        var nbResult = content.Length;

        return Task.FromResult(
            new PagedResult<Scope>
            {
                Content = content,
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
    }

    /// <inheritdoc />
    public Task<Scope[]> SearchByNames(CancellationToken cancellationToken = default, params string[] names)
    {
        if (names == null)
        {
            throw new ArgumentNullException(nameof(names));
        }

        var result = _scopes.Where(s => names.Contains(s.Name)).ToArray();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<bool> Update(Scope scope, CancellationToken cancellationToken = default)
    {
        var sc = _scopes.FirstOrDefault(s => s.Name == scope.Name);
        if (sc == null)
        {
            return Task.FromResult(false);
        }

        _scopes.Remove(sc);
        sc = sc with
        {
            Claims = scope.Claims,
            Description = scope.Description,
            IsDisplayedInConsent = scope.IsDisplayedInConsent,
            IsExposed = scope.IsExposed,
            Type = scope.Type
        };
        _scopes.Add(sc);
        return Task.FromResult(true);
    }
}