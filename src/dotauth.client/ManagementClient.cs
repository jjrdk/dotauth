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

namespace DotAuth.Client;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the management client.
/// </summary>
public sealed class ManagementClient : ClientBase, IManagementClient
{
    private ManagementClient(Func<HttpClient> client, DiscoveryInformation discoveryInformation)
        : base(client, discoveryInformation)
    {
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
            throw new ArgumentException(string.Format(ClientStrings.TheUrlIsNotWellFormed, discoveryDocumentationUri));
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
    public async Task<Option<Client>> GetClient(
        string clientId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = new Uri($"{discoveryInformation.Clients}/{clientId}")
        };
        return await GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Adds the passed client.
    /// </summary>
    /// <param name="client">The <see cref="Client"/> to add.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<Client>> AddClient(
        Client client,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var serializedJson = JsonSerializer.Serialize(client, SharedSerializerContext.Default.Client);
        var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post, RequestUri = discoveryInformation.Clients, Content = body
        };

        return await GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes the specified client.
    /// </summary>
    /// <param name="clientId">The client id.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<Client>> DeleteClient(
        string clientId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete, RequestUri = new Uri($"{discoveryInformation.Clients}/{clientId}")
        };
        return await GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    /// <param name="client">The updated <see cref="Client"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<Client>> UpdateClient(
        Client client,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var json = JsonSerializer.Serialize(client, SharedSerializerContext.Default.Client);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = discoveryInformation.Clients,
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await GetResult<Client>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets all clients.
    /// </summary>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<Client[]>> GetAllClients(
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = discoveryInformation.Clients };
        return await GetResult<Client[]>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Search for clients.
    /// </summary>
    /// <param name="searchClientParameter">The <see cref="SearchClientsRequest"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<PagedResult<Client>>> SearchClients(
        SearchClientsRequest searchClientParameter,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var serializedPostPermission =
            JsonSerializer.Serialize(searchClientParameter, SharedSerializerContext.Default.SearchClientsRequest);
        var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{discoveryInformation.Clients}/.search"),
            Content = body
        };
        return await GetResult<PagedResult<Client>>(
            request,
            authorizationHeaderValue,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the specified scope information.
    /// </summary>
    /// <param name="id">The scope id.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <exception cref="ArgumentException">If id is empty or whitespace.</exception>
    /// <returns></returns>
    public async Task<Option<Scope>> GetScope(
        string id,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(ErrorMessages.InvalidScopeId, nameof(id));
        }

        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = new Uri($"{discoveryInformation.Scopes}/{id}")
        };
        return await GetResult<Scope>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Adds the passed scope.
    /// </summary>
    /// <param name="scope">The <see cref="Scope"/> to add.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<Scope>> AddScope(
        Scope scope,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var serializedJson = JsonSerializer.Serialize(scope, SharedSerializerContext.Default.Scope);
        var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post, RequestUri = discoveryInformation.Scopes, Content = body
        };
        return await GetResult<Scope>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Registers a client with the passed details.
    /// </summary>
    /// <param name="client">The client definition to register.</param>
    /// <param name="accessToken">The access token for the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the asynchronous request.</param>
    /// <returns>A response with success or error details.</returns>
    public async Task<Option<Client>> Register(
        Client client,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var json = JsonSerializer.Serialize(client, SharedSerializerContext.Default.Client);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            RequestUri = discoveryInformation.RegistrationEndPoint
        };

        return await GetResult<Client>(request, accessToken, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Adds the passed resource owner.
    /// </summary>
    /// <param name="resourceOwner">The <see cref="AddResourceOwnerRequest"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<AddResourceOwnerResponse>> AddResourceOwner(
        AddResourceOwnerRequest resourceOwner,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (resourceOwner == null)
        {
            throw new ArgumentNullException(nameof(resourceOwner));
        }

        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = discoveryInformation.ResourceOwners,
            Content = new StringContent(
                JsonSerializer.Serialize(resourceOwner, SharedSerializerContext.Default.AddResourceOwnerRequest),
                Encoding.UTF8,
                "application/json")
        };
        return await GetResult<AddResourceOwnerResponse>(
            request,
            authorizationHeaderValue,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the specified resource owner.
    /// </summary>
    /// <param name="resourceOwnerId"></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<ResourceOwner>> GetResourceOwner(
        string resourceOwnerId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{discoveryInformation.ResourceOwners}/{resourceOwnerId}")
        };
        return await GetResult<ResourceOwner>(request, authorizationHeaderValue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes the specified resource owner.
    /// </summary>
    /// <param name="resourceOwnerId"></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option> DeleteResourceOwner(
        string resourceOwnerId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{discoveryInformation.ResourceOwners}/{resourceOwnerId}")
        };
        var result = await GetResult<object>(request, authorizationHeaderValue, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result switch
        {
            Option<object>.Error e => e.Details,
            _ => new Option.Success()
        };
    }

    /// <summary>
    /// Updates the password of the specified resource owner.
    /// </summary>
    /// <param name="updateResourceOwnerPasswordRequest">The <see cref="UpdateResourceOwnerPasswordRequest"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option> UpdateResourceOwnerPassword(
        UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (updateResourceOwnerPasswordRequest == null)
        {
            throw new ArgumentNullException(nameof(updateResourceOwnerPasswordRequest));
        }

        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var serializedJson =
            JsonSerializer.Serialize(updateResourceOwnerPasswordRequest,
                SharedSerializerContext.Default.UpdateResourceOwnerPasswordRequest);
        var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"{discoveryInformation.ResourceOwners}/password"),
            Content = body
        };
        var result = await GetResult<object>(request, authorizationHeaderValue, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result switch
        {
            Option<object>.Error e => e.Details,
            _ => new Option.Success()
        };
    }

    /// <summary>
    /// Updates the resource owner claims.
    /// </summary>
    /// <param name="updateResourceOwnerClaimsRequest"></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option> UpdateResourceOwnerClaims(
        UpdateResourceOwnerClaimsRequest updateResourceOwnerClaimsRequest,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (updateResourceOwnerClaimsRequest == null)
        {
            throw new ArgumentNullException(nameof(updateResourceOwnerClaimsRequest));
        }

        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var serializedJson =
            JsonSerializer.Serialize(updateResourceOwnerClaimsRequest,
                SharedSerializerContext.Default.UpdateResourceOwnerClaimsRequest);
        var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"{discoveryInformation.ResourceOwners}/claims"),
            Content = body
        };
        var result = await GetResult<object>(request, authorizationHeaderValue, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result switch
        {
            Option<object>.Error e => e.Details,
            _ => new Option.Success()
        };
    }

    /// <summary>
    /// Gets all resource owners.
    /// </summary>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<ResourceOwner[]>> GetAllResourceOwners(
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = discoveryInformation.ResourceOwners
        };
        return await GetResult<ResourceOwner[]>(
            request,
            authorizationHeaderValue,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Searches for resource owners.
    /// </summary>
    /// <param name="searchResourceOwnersRequest">The <see cref="SearchResourceOwnersRequest"/></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<PagedResult<ResourceOwner>>> SearchResourceOwners(
        SearchResourceOwnersRequest searchResourceOwnersRequest,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var serializedPostPermission =
            JsonSerializer.Serialize(searchResourceOwnersRequest,
                SharedSerializerContext.Default.SearchResourceOwnersRequest);
        var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{discoveryInformation.ResourceOwners}/.search"),
            Content = body
        };

        return await GetResult<PagedResult<ResourceOwner>>(
            request,
            authorizationHeaderValue,
            cancellationToken: cancellationToken);
    }
}
