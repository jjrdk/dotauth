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
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;

    public class TokenClient : ITokenClient
    {
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;
        private readonly string _authorizationValue;
        private readonly X509Certificate2 _certificate;
        private readonly HttpClient _client;
        private readonly Dictionary<string, string> _form;

        public TokenClient(
            TokenCredentials credentials,
            RequestForm form,
            HttpClient client,
            IGetDiscoveryOperation getDiscoveryOperation,
            string authorizationValue = null,
            X509Certificate2 certificate = null)
        {
            _form = credentials.Concat(form).ToDictionary(x => x.Key, x => x.Value);
            _client = client;
            _getDiscoveryOperation = getDiscoveryOperation;
            _authorizationValue = authorizationValue;
            _certificate = certificate;
        }

        public Task<GetTokenResult> GetToken(Uri tokenUri)
        {
            return PostToken(_form, tokenUri, _authorizationValue, _certificate);
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
            return await GetToken(new Uri(discoveryDocument.TokenEndPoint)).ConfigureAwait(false);
        }

        private async Task<GetTokenResult> PostToken(Dictionary<string, string> tokenRequest, Uri requestUri, string authorizationValue, X509Certificate2 certificate)
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

            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Basic " + authorizationValue);
            }
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
