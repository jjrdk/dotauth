namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Results;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MartenScopeRepository : IScopeRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        public MartenScopeRepository(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task<SearchScopeResult> Search(SearchScopesRequest parameter, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var results = await session.Query<Scope>()
                    .Where(x => x.Name.IsOneOf(parameter.ScopeNames) && x.Type.IsOneOf(parameter.ScopeTypes))
                    .Skip(parameter.StartIndex)
                    .Take(parameter.NbResults)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new SearchScopeResult
                {
                    Content = results.ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = results.Count
                };
            }
        }

        public async Task<Scope> Get(string name, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var scope = await session.LoadAsync<Scope>(name, cancellationToken).ConfigureAwait(false);

                return scope;
            }
        }

        public async Task<Scope[]> SearchByNames(CancellationToken cancellationToken = default, params string[] names)
        {
            using (var session = _sessionFactory())
            {
                var scopes = await session.Query<Scope>()
                    .Where(x => x.Name.IsOneOf(names))
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return scopes.ToArray();
            }
        }

        public async Task<Scope[]> GetAll(CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var scopes = await session.Query<Scope>()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return scopes.ToArray();
            }
        }

        public async Task<bool> Insert(Scope scope, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Store(scope);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<bool> Delete(Scope scope, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Delete(scope.Name);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<bool> Update(Scope scope, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Update(scope);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
    }
}