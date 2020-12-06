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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Client.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the management client.
    /// </summary>
    public class ManagementClient : ClientBase
    {
        private readonly DiscoveryInformation _discoveryInformation;

        private ManagementClient(Func<HttpClient> client, DiscoveryInformation discoveryInformation)
        : base(client)
        {
            _discoveryInformation = discoveryInformation;
        }

        /// <summary>
        /// Creates an instance of a management client.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to use.</param>
        /// <param name="discoveryDocumentationUri">The <see cref="Uri"/> to the discovery document.</param>
        /// <returns></returns>
        public static async Task<ManagementClient> Create(Func<HttpClient> client, Uri discoveryDocumentationUri)
        {
            if (!discoveryDocumentationUri.IsAbsoluteUri)
            {
                throw new ArgumentException(
                    string.Format(ClientStrings.TheUrlIsNotWellFormed, discoveryDocumentationUri));
            }

            var operation = new GetDiscoveryOperation(discoveryDocumentationUri, client);
            var discoveryInformation = await operation.Execute().ConfigureAwait(false);

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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_discoveryInformation.Clients + "/" + clientId)
            };
            return GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
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
                RequestUri = _discoveryInformation.Clients,
                Content = body
            };

            return GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_discoveryInformation.Clients + "/" + clientId)
            };
            return GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = _discoveryInformation.Clients,
                Content = new StringContent(
                        Serializer.Default.Serialize(client),
                        Encoding.UTF8,
                        "application/json")
            };
            return GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = _discoveryInformation.Clients };
            return GetResult<Client[]>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var serializedPostPermission = Serializer.Default.Serialize(searchClientParameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_discoveryInformation.Clients + "/.search"),
                Content = body
            };
            return GetResult<PagedResponse<Client>>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the specified scope information.
        /// </summary>
        /// <param name="id">The scope id.</param>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <exception cref="ArgumentException">If id is empty or whitespace.</exception>
        /// <returns></returns>
        public Task<GenericResponse<Scope>> GetScope(
            string id,
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(ErrorMessages.InvalidScopeId, nameof(id));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_discoveryInformation.Scopes}/{id}")
            };
            return GetResult<Scope>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var serializedJson = Serializer.Default.Serialize(scope);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _discoveryInformation.Scopes,
                Content = body
            };
            return GetResult<Scope>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Registers a client with the passed details.
        /// </summary>
        /// <param name="client">The client definition to register.</param>
        /// <param name="accessToken">The access token for the request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the asynchronous request.</param>
        /// <returns>A response with success or error details.</returns>
        public async Task<GenericResponse<Client>> Register(Client client, string accessToken, CancellationToken cancellationToken = default)
        {
            var json = Serializer.Default.Serialize(client);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                RequestUri = _discoveryInformation.RegistrationEndPoint
            };

            return await GetResult<Client>(request, accessToken, cancellationToken: cancellationToken).ConfigureAwait(false);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _discoveryInformation.ResourceOwners,
                Content = new StringContent(
                    Serializer.Default.Serialize(resourceOwner),
                    Encoding.UTF8,
                    "application/json")
            };
            return GetResult<AddResourceOwnerResponse>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/{resourceOwnerId}")
            };
            return GetResult<ResourceOwner>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/{resourceOwnerId}")
            };
            return GetResult<object>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            if (updateResourceOwnerPasswordRequest == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerPasswordRequest));
            }

            var serializedJson = Serializer.Default.Serialize(updateResourceOwnerPasswordRequest);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/password"),
                Content = body
            };
            return GetResult<object>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            if (updateResourceOwnerClaimsRequest == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerClaimsRequest));
            }

            var serializedJson = Serializer.Default.Serialize(updateResourceOwnerClaimsRequest);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"{_discoveryInformation.ResourceOwners}/claims"),
                Content = body
            };
            return GetResult<object>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets all resource owners.
        /// </summary>
        /// <param name="authorizationHeaderValue">The authorization token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public Task<GenericResponse<ResourceOwner[]>> GetAllResourceOwners(
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = _discoveryInformation.ResourceOwners
            };
            return GetResult<ResourceOwner[]>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
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
            string authorizationHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var serializedPostPermission = Serializer.Default.Serialize(searchResourceOwnersRequest);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_discoveryInformation.ResourceOwners + "/.search"),
                Content = body
            };

            return GetResult<PagedResponse<ResourceOwner>>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
        }
    }
}
