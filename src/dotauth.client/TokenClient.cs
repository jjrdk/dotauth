// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the token client.
/// </summary>
public sealed class TokenClient : ClientBase, ITokenClient
{
    private readonly AuthenticationHeaderValue? _authorizationValue;
    private readonly X509Certificate2? _certificate;
    private readonly TokenCredentials _form;
    private readonly Func<HttpClient> _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenClient"/> class.
    /// </summary>
    /// <param name="credentials">The <see cref="TokenCredentials"/>.</param>
    /// <param name="client">The <see cref="HttpClient"/> for requests.</param>
    /// <param name="authority">The <see cref="Uri"/> of the discovery document.</param>
    public TokenClient(TokenCredentials credentials, Func<HttpClient> client, Uri authority)
        : base(client, authority)
    {
        if (!authority.IsAbsoluteUri)
        {
            throw new ArgumentException(
                string.Format(ClientStrings.TheUrlIsNotWellFormed, authority));
        }

        _form = credentials;
        _client = client;
        _authorizationValue = credentials.AuthorizationValue;
        _certificate = credentials.Certificate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenClient"/> class.
    /// </summary>
    /// <param name="credentials">The <see cref="TokenCredentials"/>.</param>
    /// <param name="client">The <see cref="HttpClient"/> for requests.</param>
    /// <param name="discoveryDocumentation">The metadata information.</param>
    public TokenClient(
        TokenCredentials credentials,
        Func<HttpClient> client,
        DiscoveryInformation discoveryDocumentation)
        : base(client, discoveryDocumentation)
    {
        _form = credentials;
        _client = client;
        _authorizationValue = credentials.AuthorizationValue;
        _certificate = credentials.Certificate;
    }

    /// <summary>
    /// Gets the token.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<GrantedTokenResponse>> GetToken(
        TokenRequest tokenRequest,
        CancellationToken cancellationToken = default)
    {
        if (tokenRequest is DeviceTokenRequest deviceTokenRequest)
        {
            await Task.Delay(TimeSpan.FromSeconds(deviceTokenRequest.Interval), cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        var body = new FormUrlEncodedContent(_form.Concat(tokenRequest));
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = body,
            RequestUri = discoveryInformation.TokenEndPoint
        };

        var result =
            await GetResult<GrantedTokenResponse>(request, _authorizationValue, _certificate, cancellationToken)
                .ConfigureAwait(false);
        return result switch
        {
            Option<GrantedTokenResponse>.Error { Details.Title: ErrorCodes.AuthorizationPending } =>
                await GetToken(tokenRequest, cancellationToken).ConfigureAwait(false),
            _ => result
        };
    }

    /// <inheritdoc />
    public async Task<Option<OauthIntrospectionResponse>> Introspect(
        IntrospectionRequest introspectionRequest,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new FormUrlEncodedContent(introspectionRequest),
            RequestUri = discoveryInformation.IntrospectionEndpoint
        };

        return await GetResult<OauthIntrospectionResponse>(
                request,
                introspectionRequest.PatToken,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option<Uri>> GetAuthorization(
        AuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var uriBuilder = new UriBuilder(discoveryInformation.AuthorizationEndPoint) { Query = request.ToRequest() };
        var requestMessage = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = uriBuilder.Uri };
        requestMessage.Headers.Accept.Clear();
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));

        var response = await _client().SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

        return (int)response.StatusCode switch
        {
            < 300 => new ErrorDetails
            {
                Title = ClientStrings.NoRedirect,
                Detail = ClientStrings.NotRedirectResponse,
                Status = HttpStatusCode.UnprocessableEntity
            },
            >= 300 and < 400 => response.Headers.Location!,
            _ => (await JsonSerializer.DeserializeAsync<ErrorDetails>(
                await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                DefaultJsonSerializerOptions.Instance, cancellationToken))!
        };
    }

    /// <inheritdoc />
    public async Task<Option<DeviceAuthorizationResponse>> GetAuthorization(
        DeviceAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = discoveryInformation.DeviceAuthorizationEndPoint,
            Content = new FormUrlEncodedContent(request.ToForm())
        };
        requestMessage.Headers.Accept.Clear();
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));

        var response = await _client().SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (int)response.StatusCode switch
        {
            < 300 => (await JsonSerializer.DeserializeAsync<DeviceAuthorizationResponse>(
                stream,
                DefaultJsonSerializerOptions.Instance, cancellationToken))!,
            _ => (await JsonSerializer.DeserializeAsync<ErrorDetails>(
                stream,
                DefaultJsonSerializerOptions.Instance, cancellationToken))!,
        };
    }

    /// <inheritdoc />
    public async Task<JsonWebKeySet> GetJwks(CancellationToken cancellationToken = default)
    {
        var discoveryDoc = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = discoveryDoc.JwksUri };
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));
        var response = await _client().SendAsync(request, cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_1
        var keyJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var keyJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
        return JsonWebKeySet.Create(keyJson);
    }

    /// <summary>
    /// Sends the specified request URL.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option> RequestSms(
        ConfirmationCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var requestUri = new Uri($"{discoveryInformation.Issuer}code");

        var json = JsonSerializer.Serialize(request, DefaultJsonSerializerOptions.Instance);
        var req = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(json),
            RequestUri = requestUri
        };
        req.Content.Headers.ContentType = new MediaTypeHeaderValue(JsonMimeType);
        req.Headers.Authorization = _authorizationValue;

        var result = await _client().SendAsync(req, cancellationToken).ConfigureAwait(false);
        var content = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccessStatusCode)
        {
            return (await JsonSerializer.DeserializeAsync<ErrorDetails>(content, DefaultJsonSerializerOptions.Instance,
                cancellationToken))!;
        }

        return new Option.Success();
    }

    /// <summary>
    /// Revokes the token.
    /// </summary>
    /// <param name="revokeTokenRequest">The revoke token request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option> RevokeToken(
        RevokeTokenRequest revokeTokenRequest,
        CancellationToken cancellationToken = default)
    {
        var body = new FormUrlEncodedContent(_form.Concat(revokeTokenRequest));
        var discoveryInformation = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = body,
            RequestUri = discoveryInformation.RevocationEndPoint
        };
        if (_certificate != null)
        {
            var bytes = _certificate.RawData;
            var base64Encoded = Convert.ToBase64String(bytes);
            request.Headers.Add("X-ARR-ClientCert", base64Encoded);
        }

        request.Headers.Authorization = _authorizationValue;

        var result = await _client().SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccessStatusCode)
        {
            return new Option.Success();
        }

        var json = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync<ErrorDetails>(json, DefaultJsonSerializerOptions.Instance,
            cancellationToken))!;
    }

    /// <summary>
    /// Gets the specified user info based on the configuration URL and access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="inBody">if set to <c>true</c> [in body].</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    /// configurationUrl
    /// or
    /// accessToken
    /// </exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Option<JwtPayload>> GetUserInfo(
        string accessToken,
        bool inBody = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentNullException(nameof(accessToken));
        }

        var discoveryDocument = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage { RequestUri = discoveryDocument.UserInfoEndPoint };

        if (inBody)
        {
            request.Method = HttpMethod.Post;
            request.Content =
                new FormUrlEncodedContent(new[] { new KeyValuePair<string?, string?>("access_token", accessToken) });
        }
        else
        {
            request.Method = HttpMethod.Get;
        }

        return await GetResult<JwtPayload>(request, inBody ? null : accessToken, _certificate, cancellationToken)
            .ConfigureAwait(false);
    }
}
