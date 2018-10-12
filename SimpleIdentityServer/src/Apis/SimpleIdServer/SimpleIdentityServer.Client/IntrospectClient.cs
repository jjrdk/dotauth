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

using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Results;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net.Http;

    public interface IIntrospectClient
    {
        Task<GetIntrospectionResult> ExecuteAsync(Uri tokenUri);
        Task<GetIntrospectionResult> ResolveAsync(string discoveryDocumentationUrl);
    }

    internal class IntrospectClient : IIntrospectClient
    {
        private readonly HttpClient _client;
        private readonly RequestBuilder _requestBuilder;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public IntrospectClient(
            HttpClient client,
            RequestBuilder requestBuilder,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _client = client;
            _requestBuilder = requestBuilder;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public Task<GetIntrospectionResult> ExecuteAsync(Uri tokenUri)
        {
            if (tokenUri == null)
            {
                throw new ArgumentNullException(nameof(tokenUri));
            }

            return GetIntrospection(_requestBuilder.Content,
                tokenUri,
                _requestBuilder.AuthorizationHeaderValue);
        }

        public async Task<GetIntrospectionResult> ResolveAsync(string discoveryDocumentationUrl)
        {
            if (string.IsNullOrWhiteSpace(discoveryDocumentationUrl))
            {
                throw new ArgumentNullException(nameof(discoveryDocumentationUrl));
            }

            if (!Uri.TryCreate(discoveryDocumentationUrl, UriKind.Absolute, out Uri uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, discoveryDocumentationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri).ConfigureAwait(false);
            return await ExecuteAsync(new Uri(discoveryDocument.IntrospectionEndPoint)).ConfigureAwait(false);
        }

        private async Task<GetIntrospectionResult> GetIntrospection(Dictionary<string, string> introspectionParameter, Uri requestUri, string authorizationValue)
        {
            if (introspectionParameter == null)
            {
                throw new ArgumentNullException(nameof(introspectionParameter));
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var body = new FormUrlEncodedContent(introspectionParameter);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = requestUri
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Basic " + authorizationValue);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetIntrospectionResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json),
                    Status = result.StatusCode
                };
            }

            return new GetIntrospectionResult
            {
                ContainsError = false,
                Content = JsonConvert.DeserializeObject<IntrospectionResponse>(json)
            };
        }
    }
}
