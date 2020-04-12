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

namespace SimpleAuth.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the management client.
    /// </summary>
    public class ManagementClient
    {
        private readonly HttpClient _client;
        private readonly DiscoveryInformation _discoveryInformation;

        private ManagementClient(HttpClient client, DiscoveryInformation discoveryInformation)
        {
            _client = client;
            _discoveryInformation = discoveryInformation;
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
                    string.Format(Shared.Errors.ErrorDescriptions.TheUrlIsNotWellFormed, discoveryDocumentationUri));
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<Client>> GetClient(
            string clientId,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(_discoveryInformation.Clients + "/" + clientId)
                },
                authorizationHeaderValue);
            return GetResult<Client>(request, cancellationToken);
        }

        /// <summary>
        /// Adds the passed client.
        /// </summary>
        /// <param name="client">The <see cref="Client"/> to add.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<Client>> AddClient(
            Client client,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var serializedJson = Serializer.Default.Serialize(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = _discoveryInformation.Clients,
                    Content = body
                },
                authorizationHeaderValue);

            return GetResult<Client>(request, cancellationToken);
        }

        /// <summary>
        /// Deletes the specified client.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<Client>> DeleteClient(
            string clientId,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(_discoveryInformation.Clients + "/" + clientId)
                },
                authorizationHeaderValue);
            return GetResult<Client>(request, cancellationToken);
        }

        /// <summary>
        /// Updates an existing client.
        /// </summary>
        /// <param name="client">The updated <see cref="Client"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<Client>> UpdateClient(
            Client client,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = _discoveryInformation.Clients,
                    Content = new StringContent(
                        Serializer.Default.Serialize(client),
                        Encoding.UTF8,
                        "application/json")
                },
                authorizationHeaderValue);
            return GetResult<Client>(request, cancellationToken);
        }

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<Client[]>> GetAllClients(
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var request = PrepareRequest(
                new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = _discoveryInformation.Clients },
                authorizationHeaderValue);
            return GetResult<Client[]>(request, cancellationToken);
        }

        /// <summary>
        /// Search for clients.
        /// </summary>
        /// <param name="searchClientParameter">The <see cref="SearchClientsRequest"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<PagedResponse<Client>>> SearchClients(
            SearchClientsRequest searchClientParameter,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var serializedPostPermission = Serializer.Default.Serialize(searchClientParameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_discoveryInformation.Clients + "/.search"),
                    Content = body
                },
                authorizationHeaderValue);
            return GetResult<PagedResponse<Client>>(request, cancellationToken);
        }

        /// <summary>
        /// Gets the specified scope information.
        /// </summary>
        /// <param name="id">The scope id.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<Scope>> GetScope(
            string id,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(nameof(id));
            }

            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_discoveryInformation.Scopes}/{id}")
                },
                authorizationHeaderValue);
            return GetResult<Scope>(request, cancellationToken);
        }

        /// <summary>
        /// Adds the passed scope.
        /// </summary>
        /// <param name="scope">The <see cref="Scope"/> to add.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<Scope>> AddScope(
            Scope scope,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var serializedJson = Serializer.Default.Serialize(scope);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = _discoveryInformation.Scopes,
                    Content = body
                },
                authorizationHeaderValue);
            return GetResult<Scope>(request, cancellationToken);
        }

        /// <summary>
        /// Adds the passed resource owner.
        /// </summary>
        /// <param name="resourceOwner">The <see cref="AddResourceOwnerRequest"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<AddResourceOwnerResponse>> AddResourceOwner(
            AddResourceOwnerRequest resourceOwner,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = _discoveryInformation.ResourceOwners,
                    Content = new StringContent(
                        Serializer.Default.Serialize(resourceOwner),
                        Encoding.UTF8,
                        "application/json")
                },
                authorizationHeaderValue);
            return GetResult<AddResourceOwnerResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Gets the specified resource owner.
        /// </summary>
        /// <param name="resourceOwnerId"></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<ResourceOwner>> GetResourceOwner(
            string resourceOwnerId,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/{resourceOwnerId}")
                },
                authorizationHeaderValue);
            return GetResult<ResourceOwner>(request, cancellationToken);
        }

        /// <summary>
        /// Deletes the specified resource owner.
        /// </summary>
        /// <param name="resourceOwnerId"></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<object>> DeleteResourceOwner(
            string resourceOwnerId,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/{resourceOwnerId}")
                },
                authorizationHeaderValue);
            return GetResult<object>(request, cancellationToken);
        }

        /// <summary>
        /// Updates the password of the specified resource owner.
        /// </summary>
        /// <param name="updateResourceOwnerPasswordRequest">The <see cref="UpdateResourceOwnerPasswordRequest"/>.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<object>> UpdateResourceOwnerPassword(
            UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            if (updateResourceOwnerPasswordRequest == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerPasswordRequest));
            }

            var serializedJson = Serializer.Default.Serialize(updateResourceOwnerPasswordRequest);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/password"),
                    Content = body
                },
                authorizationHeaderValue);
            return GetResult<object>(request, cancellationToken);
        }

        /// <summary>
        /// Updates the resource owner claims.
        /// </summary>
        /// <param name="updateResourceOwnerClaimsRequest"></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<object>> UpdateResourceOwnerClaims(
            UpdateResourceOwnerClaimsRequest updateResourceOwnerClaimsRequest,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            if (updateResourceOwnerClaimsRequest == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerClaimsRequest));
            }

            var serializedJson = Serializer.Default.Serialize(updateResourceOwnerClaimsRequest);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/claims"),
                    Content = body
                },
                authorizationHeaderValue);
            return GetResult<object>(request, cancellationToken);
        }

        /// <summary>
        /// Gets all resource owners.
        /// </summary>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<ResourceOwner[]>> GetAllResourceOwners(
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var request = PrepareRequest(
                new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = _discoveryInformation.ResourceOwners },
                authorizationHeaderValue);
            return GetResult<ResourceOwner[]>(request, cancellationToken);
        }

        /// <summary>
        /// Searches for resource owners.
        /// </summary>
        /// <param name="searchResourceOwnersRequest">The <see cref="SearchResourceOwnersRequest"/></param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<PagedResponse<ResourceOwner>>> SearchResourceOwners(
            SearchResourceOwnersRequest searchResourceOwnersRequest,
            string authorizationHeaderValue = null,
            CancellationToken cancellationToken = default)
        {
            var serializedPostPermission = Serializer.Default.Serialize(searchResourceOwnersRequest);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = PrepareRequest(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_discoveryInformation.ResourceOwners + "/.search"),
                    Content = body
                },
                authorizationHeaderValue);

            return GetResult<PagedResponse<ResourceOwner>>(request, cancellationToken);
        }

        private async Task<GenericResponse<T>> GetResult<T>(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var result = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (result.IsSuccessStatusCode)
            {
                return new GenericResponse<T>
                {
                    StatusCode = result.StatusCode,
                    Content = Serializer.Default.Deserialize<T>(content)
                };
            }

            var genericResult = new GenericResponse<T>
            {
                Error = string.IsNullOrWhiteSpace(content)
                    ? new ErrorDetails { Status = result.StatusCode }
                    : Serializer.Default.Deserialize<ErrorDetails>(content),
                StatusCode = result.StatusCode
            };

            return genericResult;
        }

        private static HttpRequestMessage PrepareRequest(
            HttpRequestMessage request,
            string authorizationHeaderValue = null)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            return request;
        }
    }
}
