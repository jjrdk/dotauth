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

namespace SimpleIdentityServer.Core
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Api.Discovery;
    using Client.Errors;
    using Common.DTOs.Requests;
    using Newtonsoft.Json;

    public interface IJwksClient
    {
        Task<JsonWebKeySet> ResolveAsync(string configurationUrl);
    }

    public class JwksClient : IJwksClient
    {
        private readonly HttpClient _client;
        private readonly IDiscoveryActions _getDiscoveryOperation;

        public JwksClient(HttpClient client, IDiscoveryActions getDiscoveryOperation)
        {
            _client = client;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public async Task<JsonWebKeySet> ResolveAsync(string configurationUrl)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(string.Format(ClientErrorDescriptions.TheUrlIsNotWellFormed, configurationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.CreateDiscoveryInformation().ConfigureAwait(false);
            return await GetJwks(new Uri(discoveryDocument.JwksUri)).ConfigureAwait(false);
        }

        private async Task<JsonWebKeySet> GetJwks(Uri jwksUri)
        {
            if (jwksUri == null)
            {
                throw new ArgumentNullException(nameof(jwksUri));
            }

            var serializedContent = await _client.GetStringAsync(jwksUri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<JsonWebKeySet>(serializedContent);
        }
    }
}
