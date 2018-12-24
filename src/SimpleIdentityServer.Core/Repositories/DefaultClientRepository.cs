namespace SimpleIdentityServer.Core.Repositories
{
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Repositories;
    using Shared.Results;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal sealed class DefaultClientRepository : IClientRepository, IClientStore
    {
        private readonly List<Client> _clients;
        private readonly ClientFactory _clientFactory;

        public DefaultClientRepository(IReadOnlyCollection<Client> clients, HttpClient httpClient, IScopeStore scopeStore)
        {
            _clientFactory = new ClientFactory(httpClient, scopeStore);
            _clients = clients == null
                ? new List<Client>()
                : clients.ToList();
        }

        public Task<bool> Delete(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = _clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client == null)
            {
                return Task.FromResult(false);
            }

            var result = _clients.Remove(client);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<Client>> GetAllAsync()
        {
            return Task.FromResult(_clients.AsEnumerable());
        }

        public Task<Client> GetById(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var res = _clients.FirstOrDefault(c => c.ClientId == clientId);
            return res == null ? Task.FromResult<Client>(null) : Task.FromResult(res);
        }

        public async Task<Client> Insert(Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            if (_clients.Any(x => x.ClientId == newClient.ClientId || x.ClientName == newClient.ClientName))
            {
                throw new ArgumentException("Duplicate client");
            }
            var toInsert = await _clientFactory.Build(newClient).ConfigureAwait(false);
            _clients.Add(toInsert);
            return toInsert;
        }

        public Task<SearchClientResult> Search(SearchClientParameter newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }


            IEnumerable<Client> result = _clients;
            if (newClient.ClientIds != null && newClient.ClientIds.Any())
            {
                result = result.Where(c => newClient.ClientIds.Any(i => c.ClientId.Contains(i)));
            }

            if (newClient.ClientNames != null && newClient.ClientNames.Any())
            {
                result = result.Where(c => newClient.ClientNames.Any(n => c.ClientName.Contains(n)));
            }

            if (newClient.ClientTypes != null && newClient.ClientTypes.Any())
            {
                var clientTypes = newClient.ClientTypes.Select(t => (ApplicationTypes)t);
                result = result.Where(c => clientTypes.Contains(c.ApplicationType)).OrderBy(c => c.ClientName).ToArray();
            }

            var nbResult = result.Count();
            //if (newClient.Order != null)
            //{
            //    switch (newClient.Order.Target)
            //    {
            //        case "update_datetime":
            //            switch (newClient.Order.Type)
            //            {
            //                case OrderTypes.Asc:
            //                    result = result.OrderBy(c => c.UpdateDateTime);
            //                    break;
            //                case OrderTypes.Desc:
            //                    result = result.OrderByDescending(c => c.UpdateDateTime);
            //                    break;
            //            }
            //            break;
            //    }
            //}
            //else
            //{
            //    result = result.OrderByDescending(c => c.UpdateDateTime);
            //}

            if (newClient.IsPagingEnabled)
            {
                result = result.Skip(newClient.StartIndex).Take(newClient.Count);
            }

            return Task.FromResult(new SearchClientResult
            {
                Content = result.ToArray(),
                StartIndex = newClient.StartIndex,
                TotalResults = nbResult
            });
        }

        public async Task<Client> Update(Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            if (string.IsNullOrWhiteSpace(newClient.ClientId) || !_clients.Exists(x => x.ClientId == newClient.ClientId))
            {
                return null;
            }

            newClient = await _clientFactory.Build(newClient).ConfigureAwait(false);
            lock (_clients)
            {
                var removed = _clients.RemoveAll(x => x.ClientId == newClient.ClientId || x.ClientName == newClient.ClientName);
                if (removed != 1)
                {
                    Trace.TraceError("");
                }
                _clients.Add(newClient);
            }
            return newClient;
        }
    }
}
