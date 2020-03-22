namespace SimpleAuth.Repositories
{
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the in-memory scope repository.
    /// </summary>
    /// <seealso cref="IScopeRepository" />
    public sealed class InMemoryScopeRepository : IScopeRepository
    {
        private readonly ICollection<Scope> _scopes;

        private readonly List<Scope> _defaultScopes = new List<Scope>
        {
            new Scope
            {
                Name = "openid",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = "Access to the OpenId scope.",
                Type = ScopeTypes.ResourceOwner,
                Claims = Array.Empty<string>()
            },
            new Scope
            {
                Name = "profile",
                IsExposed = true,
                Description = "Access to the profile information.",
                Claims = new[]
                {
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
                },
                Type = ScopeTypes.ResourceOwner,
                IsDisplayedInConsent = true
            },
            new Scope
            {
                Name = "email",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = "Access to email addresses.",
                Claims = new[] {OpenIdClaimTypes.Email, OpenIdClaimTypes.EmailVerified},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "address",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = "Access to address information.",
                Claims = new[] {OpenIdClaimTypes.Address},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "phone",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = "Access to phone information.",
                Claims = new[] {OpenIdClaimTypes.PhoneNumber, OpenIdClaimTypes.PhoneNumberVerified},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "role",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = "Access to your roles.",
                Claims = new[] {OpenIdClaimTypes.Role},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Role},
                Name = "register_client",
                IsExposed = false,
                IsDisplayedInConsent = true,
                Description = "Register a client",
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Role},
                Name = "manage_profile",
                IsExposed = false,
                IsDisplayedInConsent = true,
                Description = "Manage the user's profiles",
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Role},
                Name = "manage_account_filtering",
                IsExposed = false,
                IsDisplayedInConsent = true,
                Description = "Manage the account filtering.",
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Role},
                Description = "Manage server resources.",
                IsDisplayedInConsent = true,
                IsExposed = true,
                Name = "manager",
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Subject},
                Description = "Manage user managed resources and policies.",
                IsDisplayedInConsent = true,
                IsExposed = true,
                Name = "uma_protection",
                Type = ScopeTypes.ProtectedApi
            }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryScopeRepository"/> class.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        /// <param name="includeDefaultScopes">Include default scope definitions during instantiation.</param>
        public InMemoryScopeRepository(IReadOnlyCollection<Scope> scopes = null, bool includeDefaultScopes = true)
        {
            _scopes = scopes == null || scopes.Count == 0
                ? _defaultScopes
                : scopes.Concat(includeDefaultScopes ? _defaultScopes.AsEnumerable() : Array.Empty<Scope>())
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
        public Task<Scope> Get(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var scope = _scopes.FirstOrDefault(s => s.Name == name);

            return Task.FromResult(scope);
        }

        /// <inheritdoc />
        public Task<bool> Insert(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                return Task.FromResult(false);
            }

            if (_scopes.Any(x => x.Name == scope.Name))
            {
                return Task.FromResult(false);
            }

            _scopes.Add(scope);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<GenericResult<Scope>> Search(
            SearchScopesRequest parameter,
            CancellationToken cancellationToken = default)
        {
            if (parameter == null)
            {
                return null;
            }

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
                new GenericResult<Scope>
                {
                    Content = content, StartIndex = parameter.StartIndex, TotalResults = nbResult
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
            if (scope == null)
            {
                return Task.FromResult(false);
            }

            var sc = _scopes.FirstOrDefault(s => s.Name == scope.Name);
            if (sc == null)
            {
                return Task.FromResult(false);
            }

            sc.Claims = scope.Claims;
            sc.Description = scope.Description;
            sc.IsDisplayedInConsent = scope.IsDisplayedInConsent;
            sc.IsExposed = scope.IsExposed;
            sc.Type = scope.Type;
            return Task.FromResult(true);
        }
    }
}
