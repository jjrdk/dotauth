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
    using Newtonsoft.Json;
    using Results;
    using Shared.Responses;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the introspection client.
    /// </summary>
    public class IntrospectClient
    {
        private readonly HttpClient _client;
        private readonly DiscoveryInformation _discoveryInformation;
        private readonly string _authorizationValue;
        private readonly TokenCredentials _form;

        private IntrospectClient(
            TokenCredentials credentials,
            HttpClient client,
            DiscoveryInformation discoveryInformation,
            string authorizationValue = null)
        {
            _form = credentials;
            _client = client;
            _discoveryInformation = discoveryInformation;
            _authorizationValue = authorizationValue;
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        /// <param name="client">The client.</param>
        /// <param name="discoveryDocumentationUri">The discovery documentation URI.</param>
        /// <param name="authorizationValue">The authorization value.</param>
        /// <returns></returns>
        public static async Task<IntrospectClient> Create(
            TokenCredentials credentials,
            HttpClient client,
            Uri discoveryDocumentationUri,
            string authorizationValue = null)
        {
            var operation = new GetDiscoveryOperation(client);
            var document = await operation.Execute(discoveryDocumentationUri).ConfigureAwait(false);

            return new IntrospectClient(credentials, client, document, authorizationValue);
        }

        /// <summary>
        /// Executes the specified introspection request.
        /// </summary>
        /// <param name="introspectionRequest">The introspection request.</param>
        /// <returns></returns>
        public async Task<BaseSidContentResult<IntrospectionResponse>> Introspect(IntrospectionRequest introspectionRequest)
        {
            var body = new FormUrlEncodedContent(_form.Concat(introspectionRequest));
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri(_discoveryInformation.IntrospectionEndPoint)
            };
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
                return new BaseSidContentResult<IntrospectionResponse>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json),
                    Status = result.StatusCode
                };
            }

            return new BaseSidContentResult<IntrospectionResponse>
            {
                ContainsError = false,
                Content = JsonConvert.DeserializeObject<IntrospectionResponse>(json)
            };
        }
    }
}
