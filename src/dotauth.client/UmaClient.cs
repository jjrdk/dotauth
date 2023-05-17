// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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
/// Defines the UMA client.
/// </summary>
public sealed class UmaClient : ClientBase, IUmaPermissionClient, IUmaResourceSetClient, IIntrospectionClient
{
    private readonly Uri _configurationUri;
    private UmaConfiguration? _umaConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmaClient"/> class.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to use for requests.</param>
    /// <param name="authorityUri">The <see cref="Uri"/> of the UMA authority.</param>
    public UmaClient(Func<HttpClient> client, Uri authorityUri)
        : base(client, authorityUri)
    {
        Authority = authorityUri;
        var builder = new UriBuilder(
            authorityUri.Scheme,
            authorityUri.Host,
            authorityUri.Port,
            "/.well-known/uma2-configuration");
        _configurationUri = builder.Uri;
    }

    /// <inheritdoc />
    public Uri Authority { get; }

    /// <inheritdoc />
    public async Task<Option<IReadOnlyList<Ticket>>> ApproveTicket(
        string ticketId,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var config = await GetUmaConfiguration(cancellationToken);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(config.PermissionEndpoint, $"{ticketId}/approve")
        };
        var option = await GetResult<Ticket[]>(request, accessToken, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return (option) switch
        {
            Option<Ticket[]>.Error error => error.Details,
            Option<Ticket[]>.Result result => result.Item,
            _ => throw new InvalidOperationException("Invalid option type")
        };
    }

    /// <inheritdoc />
    public async Task<Option<UmaIntrospectionResponse>> Introspect(
        IntrospectionRequest introspectionRequest,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new FormUrlEncodedContent(introspectionRequest),
            RequestUri = discoveryInformation.IntrospectionEndpoint
        };

        return await GetResult<UmaIntrospectionResponse>(request, introspectionRequest.PatToken,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option<TicketResponse>> RequestPermission(
        string token,
        CancellationToken cancellationToken = default,
        params PermissionRequest[] requests)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException(nameof(token));
        }

        var configuration = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var url = configuration.PermissionEndpoint.AbsoluteUri;

        if (requests.Length > 1)
        {
            url += url.EndsWith("/") ? "bulk" : "/bulk";
        }

        var serializedPostPermission = requests.Length > 1
            ? JsonSerializer.Serialize(requests, DefaultJsonSerializerOptions.Instance)
            : JsonSerializer.Serialize(requests[0], DefaultJsonSerializerOptions.Instance);
        var body = new StringContent(serializedPostPermission, Encoding.UTF8, JsonMimeType);
        var httpRequest =
            new HttpRequestMessage { Method = HttpMethod.Post, Content = body, RequestUri = new Uri(url) };
        return await GetResult<TicketResponse>(httpRequest, token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<UmaConfiguration> GetUmaDocument(CancellationToken cancellationToken = default)
    {
        return GetUmaConfiguration(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Option<UpdateResourceSetResponse>> UpdateResourceSet(
        ResourceSet request,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException(nameof(token));
        }

        var configuration = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var serializedPostResourceSet = JsonSerializer.Serialize(request, DefaultJsonSerializerOptions.Instance);
        var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, JsonMimeType);
        var httpRequest = new HttpRequestMessage
        {
            Content = body,
            Method = HttpMethod.Put,
            RequestUri = configuration.ResourceRegistrationEndpoint
        };
        return await GetResult<UpdateResourceSetResponse>(httpRequest, token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option<AddResourceSetResponse>> AddResourceSet(
        ResourceSet request,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException(nameof(token));
        }

        var serializedPostResourceSet = JsonSerializer.Serialize(request, DefaultJsonSerializerOptions.Instance);
        var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, JsonMimeType);
        var umaConfiguration = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var httpRequest = new HttpRequestMessage
        {
            Content = body,
            Method = HttpMethod.Post,
            RequestUri = umaConfiguration.ResourceRegistrationEndpoint
        };
        return await GetResult<AddResourceSetResponse>(httpRequest, token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option> DeleteResource(
        string resourceSetId,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceSetId))
        {
            throw new ArgumentNullException(nameof(resourceSetId));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(ClientStrings.InvalidToken, nameof(token));
        }

        var configuration = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var resourceSetUrl = configuration.ResourceRegistrationEndpoint.AbsoluteUri;
        resourceSetUrl += resourceSetUrl.EndsWith("/") ? resourceSetId : "/" + resourceSetId;

        var request = new HttpRequestMessage { Method = HttpMethod.Delete, RequestUri = new Uri(resourceSetUrl) };
        var result = await GetResult<object>(request, token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result switch
        {
            Option<object>.Error e => e.Details,
            _ => new Option.Success()
        };
    }

    /// <inheritdoc />
    public async Task<Option<string[]>> GetAllOwnResourceSets(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(ClientStrings.InvalidToken, nameof(token));
        }

        var configuration = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = configuration.ResourceRegistrationEndpoint
        };
        return await GetResult<string[]>(request, token, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option<ResourceSet>> GetResourceSet(
        string resourceSetId,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceSetId))
        {
            throw new ArgumentException(ErrorMessages.InvalidResourcesetId, nameof(resourceSetId));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(ClientStrings.InvalidToken, nameof(token));
        }

        var configuration = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var resourceSetUrl = configuration.ResourceRegistrationEndpoint.AbsoluteUri;

        resourceSetUrl += resourceSetUrl.EndsWith("/") ? resourceSetId : "/" + resourceSetId;

        var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(resourceSetUrl) };
        return await GetResult<ResourceSet>(request, token, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option<PagedResult<ResourceSetDescription>>> SearchResources(
        SearchResourceSet parameter,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(ClientStrings.InvalidToken, nameof(token));
        }

        var configuration = await GetUmaConfiguration(cancellationToken).ConfigureAwait(false);
        var url = $"{configuration.ResourceRegistrationEndpoint}/.search";

        var serializedPostPermission = JsonSerializer.Serialize(parameter, DefaultJsonSerializerOptions.Instance);
        var body = new StringContent(serializedPostPermission, Encoding.UTF8, JsonMimeType);
        var request = new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(url), Content = body };
        return await GetResult<PagedResult<ResourceSetDescription>>(request, token,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<UmaConfiguration> GetUmaConfiguration(CancellationToken cancellationToken)
    {
        if (_umaConfiguration != null)
        {
            return _umaConfiguration;
        }

        var result = await GetResult<UmaConfiguration>(
            new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = _configurationUri },
            (AuthenticationHeaderValue?)null,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result is Option<UmaConfiguration>.Error e)
        {
            throw new Exception(e.Details.Detail);
        }

        _umaConfiguration = ((Option<UmaConfiguration>.Result)result).Item;

        return _umaConfiguration;
    }
}
