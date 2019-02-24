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

namespace SimpleAuth.Client
{
    using Newtonsoft.Json;
    using Results;
    using Shared.Requests;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the authorization client.
    /// </summary>
    public class AuthorizationClient
    {
        private readonly HttpClient _client;
        private readonly DiscoveryInformation _discoveryInformation;

        private AuthorizationClient(HttpClient client, DiscoveryInformation discoveryInformation)
        {
            _client = client;
            _discoveryInformation = discoveryInformation;
        }

        /// <summary>
        /// Creates the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="discoveryUrl">The discovery URL.</param>
        /// <returns></returns>
        public static async Task<AuthorizationClient> Create(HttpClient client, Uri discoveryUrl)
        {
            var discoveryOperation = new GetDiscoveryOperation(client);
            var information = await discoveryOperation.Execute(discoveryUrl).ConfigureAwait(false);

            return new AuthorizationClient(client, information);
        }

        /// <summary>
        /// Gets the authorization.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">request</exception>
        public async Task<GetAuthorizationResult> GetAuthorization(AuthorizationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var uriBuilder = new UriBuilder(_discoveryInformation.AuthorizationEndPoint) { Query = request.ToRequest() };
            var response = await _client.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if ((int)response.StatusCode < 400)
            {
                return new GetAuthorizationResult { ContainsError = false, Location = response.Headers.Location };
            }
            return new GetAuthorizationResult
            {
                ContainsError = true,
                Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content),
                Status = response.StatusCode
            };

        }
    }
}
