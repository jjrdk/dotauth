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

namespace SimpleAuth.Uma.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.Responses;

    internal class GetConfigurationOperation
    {
        private readonly HttpClient _httpClient;

        public GetConfigurationOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<UmaConfigurationResponse> Execute(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var result = await _httpClient.GetStringAsync(configurationUri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<UmaConfigurationResponse>(result);
        }
    }
}
