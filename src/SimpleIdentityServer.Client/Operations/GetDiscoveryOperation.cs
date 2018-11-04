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

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using Shared.Responses;

    internal class GetDiscoveryOperation : IGetDiscoveryOperation
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly Dictionary<string, DiscoveryInformation> _cache = new Dictionary<string, DiscoveryInformation>();
        private readonly HttpClient _httpClient;

        public GetDiscoveryOperation(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DiscoveryInformation> ExecuteAsync(Uri discoveryDocumentationUri)
        {
            if (discoveryDocumentationUri == null)
            {
                throw new ArgumentNullException(nameof(discoveryDocumentationUri));
            }

            var key = discoveryDocumentationUri.ToString();
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                if (_cache.TryGetValue(key, out var doc))
                {
                    return doc;
                }

                var serializedContent =
                    await _httpClient.GetStringAsync(discoveryDocumentationUri).ConfigureAwait(false);
                doc = JsonConvert.DeserializeObject<DiscoveryInformation>(serializedContent);
                _cache.Add(key, doc);
                return doc;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
