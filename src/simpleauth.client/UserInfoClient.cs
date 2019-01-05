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
    using Errors;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Operations;
    using Results;
    using Shared;
    using Shared.Responses;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal class UserInfoClient : IUserInfoClient
    {
        private readonly HttpClient _client;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public UserInfoClient(
            HttpClient client,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _client = client;
            _getDiscoveryOperation = getDiscoveryOperation ?? throw new ArgumentNullException(nameof(getDiscoveryOperation));
        }

        public async Task<GetUserInfoResult> Resolve(string configurationUrl, string accessToken, bool inBody = false)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, configurationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri).ConfigureAwait(false);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(discoveryDocument.UserInfoEndPoint)
            };
            request.Headers.Add("Accept", "application/json");

            if (inBody)
            {
                request.Method = HttpMethod.Post;
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {
                        GrantedTokenNames.AccessToken, accessToken
                    }
                });
            }
            else
            {
                request.Method = HttpMethod.Get;
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }

            var serializedContent = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await serializedContent.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                serializedContent.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetUserInfoResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json),
                    Status = serializedContent.StatusCode
                };
            }

            var contentType = serializedContent.Content.Headers.ContentType;
            if (contentType?.Parameters != null && contentType.MediaType == "application/jwt")
            {
                return new GetUserInfoResult
                {
                    ContainsError = false,
                    JwtToken = json
                };
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                return new GetUserInfoResult
                {
                    ContainsError = false,
                    Content = string.IsNullOrWhiteSpace(json) ? null : JObject.Parse(json)
                };
            }
            return new GetUserInfoResult
            {
                ContainsError = true,
                Error = new ErrorResponseWithState
                {
                    Error = "invalid_token",
                    ErrorDescription = "Not a valid resource owner token"
                }
            };
        }
    }
}
