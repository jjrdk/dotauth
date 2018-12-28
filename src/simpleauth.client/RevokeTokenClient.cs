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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Errors;
    using Newtonsoft.Json;
    using Operations;
    using Results;
    using Shared.Responses;

    internal class RevokeTokenClient : IRevokeTokenClient
    {
        private readonly Dictionary<string, string> _form;
        private readonly HttpClient _client;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;
        private readonly string _authorizationValue;
        private readonly X509Certificate2 _certificate;

        public RevokeTokenClient(
            TokenCredentials credentials,
            RevokeTokenRequest request,
            HttpClient client,
            IGetDiscoveryOperation getDiscoveryOperation,
            string authorizationValue = null,
            X509Certificate2 certificate = null)
        {
            _form = credentials.Concat(request).ToDictionary(x => x.Key, x => x.Value);
            _client = client;
            _getDiscoveryOperation = getDiscoveryOperation;
            _authorizationValue = authorizationValue;
            _certificate = certificate;
        }

        public async Task<GetRevokeTokenResult> ResolveAsync(string discoveryDocumentationUrl)
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
            return await ExecuteAsync(new Uri(discoveryDocument.RevocationEndPoint)).ConfigureAwait(false);
        }

        public async Task<GetRevokeTokenResult> ExecuteAsync(Uri tokenUri)
        {
            if (tokenUri == null)
            {
                throw new ArgumentNullException(nameof(tokenUri));
            }

            var body = new FormUrlEncodedContent(_form);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = tokenUri
            };
            if (_certificate != null)
            {
                var bytes = _certificate.RawData;
                var base64Encoded = Convert.ToBase64String(bytes);
                request.Headers.Add("X-ARR-ClientCert", base64Encoded);
            }
            if (!string.IsNullOrWhiteSpace(_authorizationValue))
            {
                request.Headers.Add("Authorization", "Basic " + _authorizationValue);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetRevokeTokenResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json),
                    Status = result.StatusCode
                };
            }

            return new GetRevokeTokenResult();
        }

    }
}
