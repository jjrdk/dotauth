namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the Marten based scope repository.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.IScopeRepository" />
    public class MartenScopeRepository : IScopeRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenScopeRepository(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<GenericResult<Scope>> Search(SearchScopesRequest parameter, CancellationToken cancellationToken = default)
        {
            if (parameter == null)
            {
                return new GenericResult<Scope>();
            }

            using (var session = _sessionFactory())
            {
                var results = await session.Query<Scope>()
                    .Where(x => x.Name.IsOneOf(parameter.ScopeNames) && x.Type.IsOneOf(parameter.ScopeTypes))
                    .Skip(parameter.StartIndex)
                    .Take(parameter.NbResults)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new GenericResult<Scope>
                {
                    Content = results.ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = results.Count
                };
            }
        }

        /// <inheritdoc />
        public async Task<Scope> Get(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            using (var session = _sessionFactory())
            {
                var scope = await session.LoadAsync<Scope>(name, cancellationToken).ConfigureAwait(false);

                return scope;
            }
        }

        /// <inheritdoc />
        public async Task<Scope[]> SearchByNames(CancellationToken cancellationToken = default, params string[] names)
        {
            if (names == null)
            {
                return Array.Empty<Scope>();
            }

            using (var session = _sessionFactory())
            {
                var scopes = await session.Query<Scope>()
                    .Where(x => x.Name.IsOneOf(names))
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return scopes.ToArray();
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async Task<bool> Insert(Scope scope, CancellationToken cancellationToken = default)
        {
            if (scope == null)
            {
                return false;
            }

            using (var session = _sessionFactory())
            {
                session.Store(scope);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Delete(Scope scope, CancellationToken cancellationToken = default)
        {
            if (scope == null)
            {
                return false;
            }

            using (var session = _sessionFactory())
            {
                session.Delete(scope.Name);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Update(Scope scope, CancellationToken cancellationToken = default)
        {
            if (scope == null)
            {
                return false;
            }

            using (var session = _sessionFactory())
            {
                session.Update(scope);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
    }
}