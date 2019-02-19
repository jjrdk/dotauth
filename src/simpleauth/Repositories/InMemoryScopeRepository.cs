namespace SimpleAuth.Repositories
{
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Results;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class InMemoryScopeRepository : IScopeRepository
    {
        private readonly ICollection<Scope> _scopes;

        private readonly List<Scope> _defaultScopes = new List<Scope>
        {
            new Scope
            {
                Name = "openid",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "access to the openid scope",
                Type = ScopeTypes.ProtectedApi,
                Claims = Array.Empty<string>()
            },
            new Scope
            {
                Name = "profile",
                IsExposed = true,
                IsOpenIdScope = true,
                Description = "Access to the profile",
                Claims = new []
                {
                    JwtConstants.OpenIdClaimTypes.Name,
                    JwtConstants.OpenIdClaimTypes.FamilyName,
                    JwtConstants.OpenIdClaimTypes.GivenName,
                    JwtConstants.OpenIdClaimTypes.MiddleName,
                    JwtConstants.OpenIdClaimTypes.NickName,
                    JwtConstants.OpenIdClaimTypes.PreferredUserName,
                    JwtConstants.OpenIdClaimTypes.Profile,
                    JwtConstants.OpenIdClaimTypes.Picture,
                    JwtConstants.OpenIdClaimTypes.WebSite,
                    JwtConstants.OpenIdClaimTypes.Gender,
                    JwtConstants.OpenIdClaimTypes.BirthDate,
                    JwtConstants.OpenIdClaimTypes.ZoneInfo,
                    JwtConstants.OpenIdClaimTypes.Locale,
                    JwtConstants.OpenIdClaimTypes.UpdatedAt
                },
                Type = ScopeTypes.ResourceOwner,
                IsDisplayedInConsent = true
            },
            new Scope
            {
                Name = "email",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the email",
                Claims = new []
                {
                    JwtConstants.OpenIdClaimTypes.Email,
                    JwtConstants.OpenIdClaimTypes.EmailVerified
                },
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "address",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the address",
                Claims = new [] {JwtConstants.OpenIdClaimTypes.Address},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "phone",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the phone",
                Claims = new []
                {
                    JwtConstants.OpenIdClaimTypes.PhoneNumber,
                    JwtConstants.OpenIdClaimTypes.PhoneNumberVerified
                },
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "role",
                IsExposed = true,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Access to your roles",
                Claims = new [] {JwtConstants.OpenIdClaimTypes.Role},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "register_client",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Register a client",
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Name = "manage_profile",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Manage the user's profiles",
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Name = "manage_account_filtering",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Manage the account filtering",
                Type = ScopeTypes.ProtectedApi
            }
        };

        public InMemoryScopeRepository(IReadOnlyCollection<Scope> scopes = null)
        {
            _scopes = scopes == null || scopes.Count == 0 ? _defaultScopes : scopes.ToList();
        }

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

        public Task<Scope[]> GetAll(CancellationToken cancellationToken = default)
        {
            var res = _scopes.ToArray();
            return Task.FromResult(res);
        }

        public Task<Scope> Get(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var scope = _scopes.FirstOrDefault(s => s.Name == name);

            return Task.FromResult(scope);
        }

        public Task<bool> Insert(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (_scopes.Any(x => x.Name == scope.Name))
            {
                return Task.FromResult(false);
            }

            scope.CreateDateTime = DateTime.UtcNow;
            _scopes.Add(scope);
            return Task.FromResult(true);
        }

        public Task<SearchScopeResult> Search(SearchScopesRequest parameter, CancellationToken cancellationToken = default)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
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

            var nbResult = result.Count();
            result = parameter.Descending
                ? result.OrderByDescending(c => c.UpdateDateTime)
                : result.OrderBy(c => c.UpdateDateTime);

            if (parameter.NbResults > 0)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.NbResults);
            }

            return Task.FromResult(
                new SearchScopeResult {Content = result.ToArray(), StartIndex = parameter.StartIndex, TotalResults = nbResult});
        }

        public Task<Scope[]> SearchByNames(CancellationToken cancellationToken = default, params string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            var result = _scopes.Where(s => names.Contains(s.Name)).ToArray();
            return Task.FromResult(result);
        }

        public Task<bool> Update(Scope scope, CancellationToken cancellationToken = default)
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

            sc.Claims = scope.Claims;
            sc.Description = scope.Description;
            sc.IsDisplayedInConsent = scope.IsDisplayedInConsent;
            sc.IsExposed = scope.IsExposed;
            sc.IsOpenIdScope = scope.IsOpenIdScope;
            sc.Type = scope.Type;
            sc.UpdateDateTime = DateTime.UtcNow;
            return Task.FromResult(true);
        }
    }
}
