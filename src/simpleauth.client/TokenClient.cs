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

namespace SimpleAuth.Client
{
    using Results;
    using Shared.Responses;
    using SimpleAuth.Shared;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the token client.
    /// </summary>
    public class TokenClient
    {
        private readonly GetDiscoveryOperation _discoveryOperation;
        private readonly string _authorizationValue;
        private readonly X509Certificate2 _certificate;
        private readonly HttpClient _client;
        private readonly Uri _discoveryDocumentationUrl;
        private readonly TokenCredentials _form;
        private DiscoveryInformation _discovery;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClient"/> class.
        /// </summary>
        /// <param name="credentials">The <see cref="TokenCredentials"/>.</param>
        /// <param name="client">The <see cref="HttpClient"/> for requests.</param>
        /// <param name="discoveryDocumentationUrl">The <see cref="Uri"/> of the discovery document.</param>
        public TokenClient(TokenCredentials credentials, HttpClient client, Uri discoveryDocumentationUrl)
        {
            if (!discoveryDocumentationUrl.IsAbsoluteUri)
            {
                throw new ArgumentException(
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, discoveryDocumentationUrl));
            }

            _form = credentials;
            _client = client;
            _discoveryDocumentationUrl = discoveryDocumentationUrl;
            _authorizationValue = credentials.AuthorizationValue;
            _certificate = credentials.Certificate;
            _discoveryOperation = new GetDiscoveryOperation(client);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClient"/> class.
        /// </summary>
        /// <param name="credentials">The <see cref="TokenCredentials"/>.</param>
        /// <param name="client">The <see cref="HttpClient"/> for requests.</param>
        /// <param name="discoveryDocumentation">The metadata information.</param>
        public TokenClient(TokenCredentials credentials, HttpClient client, DiscoveryInformation discoveryDocumentation)
        {
            _form = credentials;
            _client = client;
            _authorizationValue = credentials.AuthorizationValue;
            _certificate = credentials.Certificate;
            _discovery = discoveryDocumentation;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <returns></returns>
        public async Task<BaseSidContentResult<GrantedTokenResponse>> GetToken(TokenRequest tokenRequest)
        {
            var body = new FormUrlEncodedContent(_form.Concat(tokenRequest));
            var discoveryInformation = await GetDiscoveryInformation().ConfigureAwait(false);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri(discoveryInformation.TokenEndPoint)
            };
            if (_certificate != null)
            {
                var bytes = _certificate.RawData;
                var base64Encoded = Convert.ToBase64String(bytes);
                request.Headers.Add("X-ARR-ClientCert", base64Encoded);
            }

            if (!string.IsNullOrWhiteSpace(_authorizationValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authorizationValue);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                return new BaseSidContentResult<GrantedTokenResponse>
                {
                    HasError = true,
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content),
                    Status = result.StatusCode
                };
            }

            return new BaseSidContentResult<GrantedTokenResponse>
            {
                Content = Serializer.Default.Deserialize<GrantedTokenResponse>(content)
            };
        }

        /// <summary>
        /// Gets the public web keys.
        /// </summary>
        /// <returns>The public <see cref="JsonWebKeySet"/> as a <see cref="Task{TResult}"/>.</returns>
        public async Task<JsonWebKeySet> GetJwks()
        {
            var discoveryDoc = await GetDiscoveryInformation().ConfigureAwait(false);
            var keyJson = await _client.GetStringAsync(discoveryDoc.JwksUri).ConfigureAwait(false);
            return JsonWebKeySet.Create(keyJson);
        }

        /// <summary>
        /// Sends the specified request URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<GenericResponse<object>> RequestSms(
            ConfirmationCodeRequest request)
        {
            var discoveryInformation = await GetDiscoveryInformation().ConfigureAwait(false);
            var requestUri = new Uri(discoveryInformation.Issuer + "/code");

            var json = Serializer.Default.Serialize(request);
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = requestUri
            };
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(_authorizationValue))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authorizationValue);
            }

            var result = await _client.SendAsync(req).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                return new GenericResponse<object>
                {
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new GenericResponse<object>();
        }

        /// <summary>
        /// Revokes the token.
        /// </summary>
        /// <param name="revokeTokenRequest">The revoke token request.</param>
        /// <returns></returns>
        public async Task<RevokeTokenResult> RevokeToken(RevokeTokenRequest revokeTokenRequest)
        {
            var body = new FormUrlEncodedContent(_form.Concat(revokeTokenRequest));
            var discoveryInformation = await GetDiscoveryInformation().ConfigureAwait(false);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri(discoveryInformation.RevocationEndPoint)
            };
            if (_certificate != null)
            {
                var bytes = _certificate.RawData;
                var base64Encoded = Convert.ToBase64String(bytes);
                request.Headers.Add("X-ARR-ClientCert", base64Encoded);
            }

            if (!string.IsNullOrWhiteSpace(_authorizationValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authorizationValue);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                return new RevokeTokenResult
                {
                    HasError = true,
                    Error = Serializer.Default.Deserialize<ErrorDetails>(json),
                    Status = result.StatusCode
                };
            }

            return new RevokeTokenResult { Status = result.StatusCode };
        }

        private async Task<DiscoveryInformation> GetDiscoveryInformation()
        {
            return _discovery ??= await _discoveryOperation.Execute(_discoveryDocumentationUrl).ConfigureAwait(false);
        }
    }
}
