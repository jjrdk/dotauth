#region copyright
// Copyright 2015 Habart Thierry
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
#endregion

using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    using Core.Common.DTOs.Responses;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;

    public interface ITokenClient
    {
        Task<GetTokenResult> ExecuteAsync(Uri tokenUri);
        Task<GetTokenResult> ResolveAsync(string discoveryDocumentationUrl);
    }

    internal class TokenClient : ITokenClient
    {
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;
        private readonly HttpClient _client;
        private readonly RequestBuilder _requestBuilder;

        public TokenClient(
            HttpClient client,
            RequestBuilder requestBuilder,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _client = client;
            _requestBuilder = requestBuilder;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public Task<GetTokenResult> ExecuteAsync(Uri tokenUri)
        {
            if (tokenUri == null)
            {
                throw new ArgumentNullException(nameof(tokenUri));
            }

            if (_requestBuilder.Certificate != null)
            {
                return PostToken(
                    _requestBuilder.Content,
                    tokenUri,
                    _requestBuilder.AuthorizationHeaderValue,
                    _requestBuilder.Certificate);
            }

            return PostToken(
                _requestBuilder.Content,
                tokenUri,
                _requestBuilder.AuthorizationHeaderValue);
        }

        public async Task<GetTokenResult> ResolveAsync(string discoveryDocumentationUrl)
        {
            if (string.IsNullOrWhiteSpace(discoveryDocumentationUrl))
            {
                throw new ArgumentNullException(nameof(discoveryDocumentationUrl));
            }

            if (!Uri.TryCreate(discoveryDocumentationUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, discoveryDocumentationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri).ConfigureAwait(false);
            return await ExecuteAsync(new Uri(discoveryDocument.TokenEndPoint)).ConfigureAwait(false);
        }

        private async Task<GetTokenResult> PostToken(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue, X509Certificate2 certificate = null)
        {
            if (tokenRequest == null)
            {
                throw new ArgumentNullException(nameof(tokenRequest));
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var body = new FormUrlEncodedContent(tokenRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = requestUri
            };
            if (certificate != null)
            {
                var bytes = certificate.RawData;
                var base64Encoded = Convert.ToBase64String(bytes);
                request.Headers.Add("X-ARR-ClientCert", base64Encoded);
            }

            request.Headers.Add("Authorization", "Basic " + authorizationValue);
            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetTokenResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content),
                    Status = result.StatusCode
                };
            }

            return new GetTokenResult
            {
                Content = JsonConvert.DeserializeObject<GrantedTokenResponse>(content)
            };
        }
    }
}
