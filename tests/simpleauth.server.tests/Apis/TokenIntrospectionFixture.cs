// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Server.Tests.Apis
{
    using Client;
    using Newtonsoft.Json;
    using Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class TokenIntrospectionFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        private readonly TestOauthServerFixture _server;

        public TokenIntrospectionFixture(ITestOutputHelper outputHelper)
        {
            _server = new TestOauthServerFixture(outputHelper);
        }

        [Fact]
        public async Task When_No_Parameters_Is_Passed_To_Introspection_Edp_Then_Error_Is_Returned()
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}/introspect")
            };

            var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);
            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal("no parameter in body request", error.Detail);
        }

        [Fact]
        public async Task When_No_Valid_Parameters_Is_Passed_Then_Error_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new("invalid", "invalid")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/introspect")
            };

            var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);
            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal("no parameter in body request", error.Detail);
        }

        [Fact]
        public async Task WhenIntrospectingAndTokenDoesNotExistThenResponseShowsInactiveToken()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var introspection = await tokenClient.Introspect(
                    IntrospectionRequest.Create("invalid_token", TokenTypes.AccessToken, "pat"))
                .ConfigureAwait(false);

            Assert.False(introspection.Content.Active);
        }

        [Fact]
        public async Task When_Introspecting_AccessToken_Then_Information_Are_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var introspection = await tokenClient.Introspect(
                    IntrospectionRequest.Create(result.Content.AccessToken, TokenTypes.AccessToken, "pat"))
                .ConfigureAwait(false);

            Assert.Single(introspection.Content.Scope);
            Assert.Equal("scim", introspection.Content.Scope.First());
        }

        [Fact]
        public async Task When_Introspecting_RefreshToken_Then_Information_Are_Returned()
        {
            var tokenClient = new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var introspection = await tokenClient.Introspect(
                    IntrospectionRequest.Create(result.Content.RefreshToken, TokenTypes.RefreshToken, "pat"))
                .ConfigureAwait(false);

            Assert.Single(introspection.Content.Scope);
            Assert.Equal("scim", introspection.Content.Scope.First());
        }
    }
}
