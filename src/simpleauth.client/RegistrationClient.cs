// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS I S" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.Client
{
    using Shared.Models;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    internal class RegistrationClient
    {
        private readonly HttpClient _client;
        private readonly GetDiscoveryOperation _getDiscoveryOperation;

        public RegistrationClient(HttpClient client)
        {
            _client = client;
            _getDiscoveryOperation = new GetDiscoveryOperation(client);
        }

        public async Task<GenericResponse<Client>> Resolve(Client client, string configurationUrl, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, configurationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.Execute(uri).ConfigureAwait(false);

            var json = Serializer.Default.Serialize(client);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = new Uri(discoveryDocument.RegistrationEndPoint)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    accessToken);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return (!result.IsSuccessStatusCode)
                ? new GenericResponse<Client>
                {
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content), HttpStatus = result.StatusCode
                }
                : new GenericResponse<Client>
                {
                    HttpStatus = result.StatusCode, Content = Serializer.Default.Deserialize<Client>(content)
                };
        }
    }
}
