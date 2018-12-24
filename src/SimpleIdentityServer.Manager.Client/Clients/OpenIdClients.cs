namespace SimpleIdentityServer.Manager.Client.Clients
{
    using Configuration;
    using Results;
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    internal sealed class OpenIdClients : IOpenIdClients
    {
        private readonly IAddClientOperation _addClientOperation;
        private readonly IUpdateClientOperation _updateClientOperation;
        private readonly IGetAllClientsOperation _getAllClientsOperation;
        private readonly IDeleteClientOperation _deleteClientOperation;
        private readonly IGetClientOperation _getClientOperation;
        private readonly ISearchClientOperation _searchClientOperation;
        private readonly IGetConfigurationOperation _configurationClient;

        public OpenIdClients(IAddClientOperation addClientOperation,
            IUpdateClientOperation updateClientOperation,
            IGetAllClientsOperation getAllClientsOperation,
            IDeleteClientOperation deleteClientOperation,
            IGetClientOperation getClientOperation,
            ISearchClientOperation searchClientOperation,
            IGetConfigurationOperation configurationClient)
        {
            _addClientOperation = addClientOperation;
            _updateClientOperation = updateClientOperation;
            _getAllClientsOperation = getAllClientsOperation;
            _deleteClientOperation = deleteClientOperation;
            _getClientOperation = getClientOperation;
            _searchClientOperation = searchClientOperation;
            _configurationClient = configurationClient;
        }

        public async Task<AddClientResult> ResolveAdd(
            Uri wellKnownConfigurationUri,
            Client client,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _addClientOperation
                .ExecuteAsync(new Uri(configuration.Content.Clients), client, authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri,
            Client client,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _updateClientOperation
                .ExecuteAsync(new Uri(configuration.Content.Clients), client, authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GetClientResult> ResolveGet(Uri wellKnownConfigurationUri,
            string clientId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getClientOperation
                .ExecuteAsync(new Uri(configuration.Content.Clients + "/" + clientId), authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolveDelete(Uri wellKnownConfigurationUri,
            string clientId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _deleteClientOperation
                .ExecuteAsync(new Uri(configuration.Content.Clients + "/" + clientId), authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public Task<GetAllClientResult> GetAll(Uri clientsUri, string authorizationHeaderValue = null)
        {
            return _getAllClientsOperation.ExecuteAsync(clientsUri, authorizationHeaderValue);
        }

        public async Task<GetAllClientResult> ResolveGetAll(Uri wellKnownConfigurationUri,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await GetAll(new Uri(configuration.Content.Clients), authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<PagedResult<ClientResponse>> ResolveSearch(Uri wellKnownConfigurationUri,
            SearchClientsRequest searchClientParameter,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _searchClientOperation
                .ExecuteAsync(new Uri(configuration.Content.Clients + "/.search"),
                    searchClientParameter,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }
    }
}
