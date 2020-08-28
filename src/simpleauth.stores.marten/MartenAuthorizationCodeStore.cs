namespace SimpleAuth.Stores.Marten
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    /// <summary>
    /// Defines the marten based authorization code store.
    /// </summary>
    /// <seealso cref="IAuthorizationCodeStore" />
    public class MartenAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenAuthorizationCodeStore(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<AuthorizationCode?> Get(string code, CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            var authorizationCode = await session.LoadAsync<AuthorizationCode>(code, cancellationToken)
                .ConfigureAwait(false);

            return authorizationCode;
        }

        /// <inheritdoc />
        public async Task<bool> Add(
            AuthorizationCode authorizationCode,
            CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            session.Store(authorizationCode);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Remove(string code, CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            session.Delete<AuthorizationCode>(code);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
    }
}