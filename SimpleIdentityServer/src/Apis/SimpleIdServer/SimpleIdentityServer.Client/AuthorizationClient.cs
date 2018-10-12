// Copyright 2016 Habart Thierry
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

using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Results;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    using Core.Common.DTOs.Responses;
    using Core.Common.Serializers;
    using Newtonsoft.Json;
    using System.Net.Http;

    public interface IAuthorizationClient
    {
        Task<GetAuthorizationResult> ExecuteAsync(Uri authorizationUri, AuthorizationRequest request);
        Task<GetAuthorizationResult> ResolveAsync(string discoveryDocumentationUrl, AuthorizationRequest request);
    }

    internal class AuthorizationClient : IAuthorizationClient
    {
        private readonly HttpClient _client;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public AuthorizationClient(HttpClient client, IGetDiscoveryOperation getDiscoveryOperation)
        {
            _client = client;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public Task<GetAuthorizationResult> ExecuteAsync(Uri authorizationUri, AuthorizationRequest request)
        {
            if (authorizationUri == null)
            {
                throw new ArgumentNullException(nameof(authorizationUri));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return GetAuthorization(authorizationUri, request);
        }

        public async Task<GetAuthorizationResult> ResolveAsync(string discoveryDocumentationUrl, AuthorizationRequest request)
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
            return await ExecuteAsync(new Uri(discoveryDocument.AuthorizationEndPoint), request).ConfigureAwait(false);
        }

        private async Task<GetAuthorizationResult> GetAuthorization(Uri uri, AuthorizationRequest request)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var uriBuilder = new UriBuilder(uri);
            var pSerializer = new ParamSerializer();
            uriBuilder.Query = pSerializer.Serialize(request);
            var response = await _client.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.StatusCode >= System.Net.HttpStatusCode.BadRequest)
            {
                return new GetAuthorizationResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content),
                    Status = response.StatusCode
                };
            }

            return new GetAuthorizationResult
            {
                ContainsError = false,
                Location = response.Headers.Location
            };
        }
    }
}
