﻿// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    internal sealed class GetAllClientsOperation
    {
        private readonly HttpClient _httpClient;

        public GetAllClientsOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<Client[]>> Execute(Uri clientsUri, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = clientsUri};
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<Client[]>
                {
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Client[]> {Content = Serializer.Default.Deserialize<Client[]>(content)};
        }
    }
}
