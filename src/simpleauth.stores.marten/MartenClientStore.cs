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

    using global::Marten.Pagination;

    /// <summary>
    /// Defines the marten based client store.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.IClientRepository" />
    public class MartenClientStore : IClientRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenClientStore"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenClientStore(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<Client?> GetById(string clientId, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            var client = await session.LoadAsync<Client>(clientId, cancellationToken).ConfigureAwait(false);
            return client;
        }

        /// <inheritdoc />
        public async Task<Client[]> GetAll(CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            var clients = await session.Query<Client>().ToListAsync(cancellationToken).ConfigureAwait(false);
            return clients.ToArray();
        }

        /// <inheritdoc />
        public async Task<PagedResult<Client>> Search(
            SearchClientsRequest parameter,
            CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            var take = parameter.NbResults == 0 ? int.MaxValue : parameter.NbResults;
            var results = await session.Query<Client>()
                .Where(x => x.ClientId.IsOneOf(parameter.ClientIds))
                .ToPagedListAsync(parameter.StartIndex + 1, take, cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<Client>
            {
                Content = results.ToArray(),
                StartIndex = parameter.StartIndex,
                TotalResults = results.TotalItemCount
            };
        }

        /// <inheritdoc />
        public async Task<bool> Update(Client client, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            if (session.LoadAsync<Client>(client.ClientId, cancellationToken) != null)
            {
                return false;
            }

            session.Update(client);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Insert(Client client, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            if (session.LoadAsync<Client>(client.ClientId, cancellationToken) != null)
            {
                return false;
            }

            session.Store(client);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Delete(string clientId, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            session.Delete<Client>(clientId);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
    }
}