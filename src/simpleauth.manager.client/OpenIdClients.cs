namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Manager.Client.Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    internal sealed class OpenIdClients
    {
        private readonly HttpClient _httpClient;
        private readonly GetAllClientsOperation _getAllClientsOperation;
        private readonly DeleteClientOperation _deleteClientOperation;
        private readonly GetClientOperation _getClientOperation;
        private readonly SearchClientOperation _searchClientOperation;
        private readonly GetConfigurationOperation _configurationClient;

        public OpenIdClients(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _getAllClientsOperation = new GetAllClientsOperation(httpClient);
            _deleteClientOperation = new DeleteClientOperation(httpClient);
            _getClientOperation = new GetClientOperation(httpClient);
            _searchClientOperation = new SearchClientOperation(httpClient);
            _configurationClient = new GetConfigurationOperation(httpClient);
        }

        public async Task<GenericResponse<Client>> ResolveAdd(
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

            var configuration = await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);

            var serializedJson = JsonConvert.SerializeObject(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post, RequestUri = new Uri(configuration.Content.Clients), Content = body
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
                return new GenericResponse<Client>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Client> {Content = JsonConvert.DeserializeObject<Client>(content)};
        }

        public async Task<GenericResponse<Client>> ResolveUpdate(
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

            var configuration = await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);

            var serializedJson = JsonConvert.SerializeObject(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put, RequestUri = new Uri(configuration.Content.Clients), Content = body
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
                return new GenericResponse<Client>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Client>();
        }

        public async Task<GenericResponse<Client>> ResolveGet(
            Uri wellKnownConfigurationUri,
            string clientId,
            string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getClientOperation.Execute(
                    new Uri(configuration.Content.Clients + "/" + clientId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GenericResponse<Client>> ResolveDelete(
            Uri wellKnownConfigurationUri,
            string clientId,
            string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _deleteClientOperation.Execute(
                    new Uri(configuration.Content.Clients + "/" + clientId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        private Task<GenericResponse<ClientResponse[]>> GetAll(Uri clientsUri, string authorizationHeaderValue = null)
        {
            return _getAllClientsOperation.Execute(clientsUri, authorizationHeaderValue);
        }

        public async Task<GenericResponse<ClientResponse[]>> ResolveGetAll(
            Uri wellKnownConfigurationUri,
            string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await GetAll(new Uri(configuration.Content.Clients), authorizationHeaderValue).ConfigureAwait(false);
        }

        public async Task<PagedResult<ClientResponse>> ResolveSearch(
            Uri wellKnownConfigurationUri,
            SearchClientsRequest searchClientParameter,
            string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _searchClientOperation.Execute(
                    new Uri(configuration.Content.Clients + "/.search"),
                    searchClientParameter,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }
    }
}
