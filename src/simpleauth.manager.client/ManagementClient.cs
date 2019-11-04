// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.Manager.Client
{
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the management client.
    /// </summary>
    public class ManagementClient
    {
        private readonly AddScopeOperation _addScopeOperation;
        private readonly GetAllClientsOperation _getAllClientsOperation;
        private readonly DeleteClientOperation _deleteClientOperation;
        private readonly GetClientOperation _getClientOperation;
        private readonly SearchClientOperation _searchClientOperation;
        private readonly AddResourceOwnerOperation _addResourceOwnerOperation;
        private readonly DeleteResourceOwnerOperation _deleteResourceOwnerOperation;
        private readonly GetAllResourceOwnersOperation _getAllResourceOwnersOperation;
        private readonly GetResourceOwnerOperation _getResourceOwnerOperation;
        private readonly UpdateResourceOwnerPasswordOperation _updateResourceOwnerPasswordOperation;
        private readonly UpdateResourceOwnerClaimsOperation _updateResourceOwnerClaimsOperation;
        private readonly SearchResourceOwnersOperation _searchResourceOwnersOperation;
        private readonly HttpClient _client;
        private readonly DiscoveryInformation _discoveryInformation;

        private ManagementClient(HttpClient client, DiscoveryInformation discoveryInformation)
        {
            _client = client;
            _discoveryInformation = discoveryInformation;
            _addResourceOwnerOperation = new AddResourceOwnerOperation(client);
            _deleteResourceOwnerOperation = new DeleteResourceOwnerOperation(client);
            _getAllResourceOwnersOperation = new GetAllResourceOwnersOperation(client);
            _getResourceOwnerOperation = new GetResourceOwnerOperation(client);
            _updateResourceOwnerClaimsOperation = new UpdateResourceOwnerClaimsOperation(client);
            _updateResourceOwnerPasswordOperation = new UpdateResourceOwnerPasswordOperation(client);
            _searchResourceOwnersOperation = new SearchResourceOwnersOperation(client);
            _getAllClientsOperation = new GetAllClientsOperation(client);
            _deleteClientOperation = new DeleteClientOperation(client);
            _getClientOperation = new GetClientOperation(client);
            _searchClientOperation = new SearchClientOperation(client);
            _addScopeOperation = new AddScopeOperation(client);
        }

        /// <summary>
        /// Creates an instance of a management client.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to use.</param>
        /// <param name="discoveryDocumentationUri">The <see cref="Uri"/> to the discovery document.</param>
        /// <returns></returns>
        public static async Task<ManagementClient> Create(HttpClient client, Uri discoveryDocumentationUri)
        {
            if (!discoveryDocumentationUri.IsAbsoluteUri)
            {
                throw new ArgumentException(
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, discoveryDocumentationUri));
            }

            var operation = new GetDiscoveryOperation(client);
            var discoveryInformation = await operation.Execute(discoveryDocumentationUri).ConfigureAwait(false);

            return new ManagementClient(client, discoveryInformation);
        }

        /// <summary>
        /// Gets the specified <see cref="Client"/> information.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<Client>> GetClient(string clientId, string authorizationHeaderValue = null)
        {
            return await _getClientOperation.Execute(
                    new Uri(_discoveryInformation.Clients + "/" + clientId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Adds the passed client.
        /// </summary>
        /// <param name="client">The <see cref="Client"/> to add.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<Client>> AddClient(Client client, string authorizationHeaderValue = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var serializedJson = Serializer.Default.Serialize(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_discoveryInformation.Clients),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<Client>
                {
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Client> { Content = JsonConvert.DeserializeObject<Client>(content) };
        }

        /// <summary>
        /// Deletes the specified client.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<Client>> DeleteClient(string clientId, string authorizationHeaderValue = null)
        {
            return await _deleteClientOperation.Execute(
                    new Uri(_discoveryInformation.Clients + "/" + clientId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing client.
        /// </summary>
        /// <param name="client">The updated <see cref="Client"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<Client>> UpdateClient(Client client, string authorizationHeaderValue = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var serializedJson = Serializer.Default.Serialize(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_discoveryInformation.Clients),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<Client>
                {
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Client>();
        }

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public Task<GenericResponse<Client[]>> GetAllClients(string authorizationHeaderValue)
        {
            return _getAllClientsOperation.Execute(new Uri(_discoveryInformation.Clients), authorizationHeaderValue);
        }

        /// <summary>
        /// Search for clients.
        /// </summary>
        /// <param name="searchClientParameter">The <see cref="SearchClientsRequest"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<PagedResponse<Client>>> SearchClients(
            SearchClientsRequest searchClientParameter,
            string authorizationHeaderValue = null)
        {
            return await _searchClientOperation.Execute(
                    new Uri(_discoveryInformation.Clients + "/.search"),
                    searchClientParameter,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the specified scope information.
        /// </summary>
        /// <param name="id">The scope id.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<Scope>> GetScope(string id, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(nameof(id));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_discoveryInformation.Scopes}/{id}")
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<Scope>
                {
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Scope> { Content = JsonConvert.DeserializeObject<Scope>(content) };
        }

        /// <summary>
        /// Adds the passed scope.
        /// </summary>
        /// <param name="scope">The <see cref="Scope"/> to add.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public Task<GenericResponse<Scope>> AddScope(Scope scope, string authorizationHeaderValue = null)
        {
            return _addScopeOperation.Execute(new Uri(_discoveryInformation.Scopes), scope, authorizationHeaderValue);
        }

        /// <summary>
        /// Adds the passed resource owner.
        /// </summary>
        /// <param name="request">The <see cref="AddResourceOwnerRequest"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public Task<GenericResponse<string>> AddResourceOwner(
            AddResourceOwnerRequest request,
            string authorizationHeaderValue = null)
        {
            return _addResourceOwnerOperation.Execute(
                new Uri(_discoveryInformation.ResourceOwners),
                request,
                authorizationHeaderValue);
        }

        /// <summary>
        /// Gets the specified resource owner.
        /// </summary>
        /// <param name="resourceOwnerId"></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<ResourceOwner>> GetResourceOwner(
            string resourceOwnerId,
            string authorizationHeaderValue = null)
        {
            return await _getResourceOwnerOperation.Execute(
                    new Uri($"{_discoveryInformation.ResourceOwners}/{resourceOwnerId}"),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the specified resource owner.
        /// </summary>
        /// <param name="resourceOwnerId"></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<object>> DeleteResourceOwner(
            string resourceOwnerId,
            string authorizationHeaderValue = null)
        {
            return await _deleteResourceOwnerOperation.Execute(
                    new Uri($"{_discoveryInformation.ResourceOwners}/{resourceOwnerId}"),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the password of the specified resource owner.
        /// </summary>
        /// <param name="request">The <see cref="UpdateResourceOwnerPasswordRequest"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<object>> UpdateResourceOwnerPassword(
            UpdateResourceOwnerPasswordRequest request,
            string authorizationHeaderValue = null)
        {
            return await _updateResourceOwnerPasswordOperation.Execute(
                    new Uri($"{_discoveryInformation.ResourceOwners}/password"),
                    request,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the resource owner claims.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<object>> UpdateResourceOwnerClaims(
            UpdateResourceOwnerClaimsRequest request,
            string authorizationHeaderValue = null)
        {
            return await _updateResourceOwnerClaimsOperation.Execute(
                    new Uri($"{_discoveryInformation.ResourceOwners}/claims"),
                    request,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all resource owners.
        /// </summary>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public Task<GenericResponse<ResourceOwner[]>> GetAllResourceOwners(string authorizationHeaderValue = null)
        {
            return _getAllResourceOwnersOperation.Execute(
                new Uri(_discoveryInformation.ResourceOwners),
                authorizationHeaderValue);
        }

        /// <summary>
        /// Searches for resource owners.
        /// </summary>
        /// <param name="searchResourceOwnersRequest">The <see cref="SearchResourceOwnersRequest"/></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<PagedResponse<ResourceOwner>>> SearchResourceOwners(
            SearchResourceOwnersRequest searchResourceOwnersRequest,
            string authorizationHeaderValue = null)
        {
            return await _searchResourceOwnersOperation.Execute(
                    new Uri(_discoveryInformation.ResourceOwners + "/.search"),
                    searchResourceOwnersRequest,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }
    }
}
