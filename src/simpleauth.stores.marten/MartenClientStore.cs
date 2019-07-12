namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Linq;
    using System.Net.Http;
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
        private readonly IScopeStore _scopeRepository;
        private readonly HttpClient _httpClient;
        private readonly Func<string, Uri[]> _urlReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenClientStore"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        /// <param name="scopeRepository">The scope repository.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="urlReader">The URL reader.</param>
        public MartenClientStore(Func<IDocumentSession> sessionFactory, IScopeStore scopeRepository, HttpClient httpClient, Func<string, Uri[]> urlReader)
        {
            _sessionFactory = sessionFactory;
            _scopeRepository = scopeRepository;
            _httpClient = httpClient;
            _urlReader = urlReader;
        }

        /// <inheritdoc />
        public async Task<Client> GetById(string clientId, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var client = await session.LoadAsync<Client>(clientId, cancellationToken).ConfigureAwait(false);
                return client;
            }
        }

        /// <inheritdoc />
        public async Task<Client[]> GetAll(CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var clients = await session.Query<Client>().ToListAsync(cancellationToken).ConfigureAwait(false);
                return clients.ToArray();
            }
        }

        /// <inheritdoc />
        public async Task<GenericResult<Client>> Search(
            SearchClientsRequest parameter,
            CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                parameter.StartIndex++;
                var take = parameter.NbResults == 0 ? int.MaxValue : parameter.NbResults;
                var results = await session.Query<Client>()
                    .Where(x => x.ClientId.IsOneOf(parameter.ClientIds))
                    .ToPagedListAsync(parameter.StartIndex, take, cancellationToken)
                    .ConfigureAwait(false);

                return new GenericResult<Client>
                {
                    Content = results.ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = results.TotalItemCount
                };
            }
        }

        /// <inheritdoc />
        public async Task<Client> Update(Client client, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                if (session.LoadAsync<Client>(client.ClientId, cancellationToken) != null)
                {
                    throw new ArgumentException("Duplicate client");
                }

                var clientFactory = new ClientFactory(_httpClient, _scopeRepository, _urlReader);
                var toInsert = await clientFactory.Build(client).ConfigureAwait(false);
                session.Update(client);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return toInsert;
            }
        }

        /// <inheritdoc />
        public async Task<Client> Insert(Client client, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                if (session.LoadAsync<Client>(client.ClientId, cancellationToken) != null)
                {
                    throw new ArgumentException("Duplicate client");
                }

                var clientFactory = new ClientFactory(_httpClient, _scopeRepository, _urlReader);
                var toInsert = await clientFactory.Build(client).ConfigureAwait(false);
                session.Store(client);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return toInsert;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Delete(string clientId, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Delete<Client>(clientId);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
    }
}