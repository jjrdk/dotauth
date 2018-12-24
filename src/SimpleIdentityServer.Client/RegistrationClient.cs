// Copyright 2016 Habart Thierry
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

namespace SimpleIdentityServer.Client
{
    using SimpleIdentityServer.Client.Errors;
    using SimpleIdentityServer.Client.Operations;
    using SimpleIdentityServer.Client.Results;
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Responses;

    internal class RegistrationClient : IRegistrationClient
    {
        private readonly HttpClient _client;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public RegistrationClient(HttpClient client, IGetDiscoveryOperation getDiscoveryOperation)
        {
            _client = client;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public async Task<GetRegisterClientResult> ResolveAsync(Client client,
            string configurationUrl,
            string accessToken)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, configurationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri).ConfigureAwait(false);

            var json = JsonConvert.SerializeObject(
                client,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = new Uri(discoveryDocument.RegistrationEndPoint)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetRegisterClientResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content),
                    Status = result.StatusCode
                };
            }

            return new GetRegisterClientResult
            {
                ContainsError = false,
                Content = JsonConvert.DeserializeObject<Client>(content)
            };
        }
    }
}
