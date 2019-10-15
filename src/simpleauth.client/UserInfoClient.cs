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
    using Newtonsoft.Json.Linq;
    using Results;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the user info client.
    /// </summary>
    public class UserInfoClient
    {
        private readonly HttpClient _client;
        private readonly Uri _userInfoEndpoint;

        internal UserInfoClient(HttpClient client, Uri userInfoEndpoint)
        {
            _client = client;
            _userInfoEndpoint = userInfoEndpoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoClient"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public static async Task<UserInfoClient> Create(HttpClient client, Uri configurationUrl)
        {
            var operation = new GetDiscoveryOperation(client);
            var discoveryDoc = await operation.Execute(configurationUrl).ConfigureAwait(false);

            return new UserInfoClient(client, new Uri(discoveryDoc.UserInfoEndPoint));
        }

        /// <summary>
        /// Gets the specified user info based on the configuration URL and access token.
        /// </summary>
        /// <param name="configurationUrl">The configuration URL.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="inBody">if set to <c>true</c> [in body].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// configurationUrl
        /// or
        /// accessToken
        /// </exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<GetUserInfoResult> Get(string accessToken, bool inBody = false)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var request = new HttpRequestMessage
            {
                RequestUri = _userInfoEndpoint
            };
            request.Headers.Add("Accept", "application/json");

            if (inBody)
            {
                request.Method = HttpMethod.Post;
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {
                        "access_token", accessToken
                    }
                });
            }
            else
            {
                request.Method = HttpMethod.Get;
                request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, accessToken);
            }

            var serializedContent = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await serializedContent.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!serializedContent.IsSuccessStatusCode)
            {
                return new GetUserInfoResult
                {
                    HasError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(json),
                    Status = serializedContent.StatusCode
                };
            }

            var contentType = serializedContent.Content.Headers.ContentType;
            if (contentType?.Parameters != null && contentType.MediaType == "application/jwt")
            {
                return new GetUserInfoResult
                {
                    HasError = false,
                    JwtToken = json
                };
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                return new GetUserInfoResult
                {
                    HasError = false,
                    Content = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<JwtPayload>(json)
                };
            }
            return new GetUserInfoResult
            {
                HasError = true,
                Error = new ErrorDetails
                {
                    Title = "invalid_token",
                    Detail = "Not a valid resource owner token"
                }
            };
        }
    }
}
