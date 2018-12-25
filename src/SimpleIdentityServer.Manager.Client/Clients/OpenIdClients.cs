namespace SimpleIdentityServer.Manager.Client.Clients
{
    using Configuration;
    using Newtonsoft.Json;
    using Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class OpenIdClients : IOpenIdClients
    {
        private readonly HttpClient _httpClient;
        private readonly IGetAllClientsOperation _getAllClientsOperation;
        private readonly IDeleteClientOperation _deleteClientOperation;
        private readonly IGetClientOperation _getClientOperation;
        private readonly ISearchClientOperation _searchClientOperation;
        private readonly GetConfigurationOperation _configurationClient;

        public OpenIdClients(
            HttpClient httpClient,
            IGetAllClientsOperation getAllClientsOperation,
            IDeleteClientOperation deleteClientOperation,
            IGetClientOperation getClientOperation,
            ISearchClientOperation searchClientOperation)
        {
            _httpClient = httpClient;
            _getAllClientsOperation = getAllClientsOperation;
            _deleteClientOperation = deleteClientOperation;
            _getClientOperation = getClientOperation;
            _searchClientOperation = searchClientOperation;
            _configurationClient = new GetConfigurationOperation(httpClient);
        }

        public async Task<AddClientResult> ResolveAdd(
            Uri wellKnownConfigurationUri,
            Client client,
            string authorizationHeaderValue = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (wellKnownConfigurationUri == null)
            {
                throw new ArgumentNullException(nameof(wellKnownConfigurationUri));
            }

            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);

            var serializedJson = JsonConvert.SerializeObject(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(configuration.Content.Clients),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new AddClientResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new AddClientResult
            {
                Content = JsonConvert.DeserializeObject<Client>(content)
            };
        }

        public async Task<BaseResponse> ResolveUpdate(
            Uri wellKnownConfigurationUri,
            Client client,
            string authorizationHeaderValue = null)
        {
            if (wellKnownConfigurationUri == null)
            {
                throw new ArgumentNullException(nameof(wellKnownConfigurationUri));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);

            var serializedJson = JsonConvert.SerializeObject(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(configuration.Content.Clients),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }

        public async Task<GetClientResult> ResolveGet(
            Uri wellKnownConfigurationUri,
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
