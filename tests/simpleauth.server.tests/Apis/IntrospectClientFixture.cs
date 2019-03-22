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

    public class IntrospectClientFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        private readonly TestOauthServerFixture _server;

        public IntrospectClientFixture()
        {
            _server = new TestOauthServerFixture();
        }

        [Fact]
        public async Task When_No_Parameters_Is_Passed_To_Introspection_Edp_Then_Error_Is_Returned()
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post, RequestUri = new Uri($"{BaseUrl}/introspect")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("no parameter in body request", error.Detail);
        }

        [Fact]
        public async Task When_No_Valid_Parameters_Is_Passed_Then_Error_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("invalid", "invalid")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/introspect")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("no parameter in body request", error.Detail);
        }

        [Fact]
        public async Task When_Introspect_And_Client_Not_Authenticated_Then_Error_Is_Returned()
        {
            var introspectClient = await IntrospectClient.Create(
                    TokenCredentials.FromClientCredentials("invalid_client", "invalid_client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var introspection = await introspectClient
                .Introspect(IntrospectionRequest.Create("invalid_token", TokenTypes.AccessToken))
                .ConfigureAwait(false);

            Assert.True(introspection.ContainsError);
            Assert.Equal("invalid_client", introspection.Error.Title);
            Assert.Equal("the client doesn't exist", introspection.Error.Detail);
        }

        [Fact]
        public async Task When_Introspect_And_Token_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var introspectClient = await IntrospectClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var introspection = await introspectClient.Introspect(
                    IntrospectionRequest.Create("invalid_token", TokenTypes.AccessToken))
                .ConfigureAwait(false);

            Assert.True(introspection.ContainsError);
            Assert.Equal("invalid_token", introspection.Error.Title);
            Assert.Equal("the token is not valid", introspection.Error.Detail);
        }

        [Fact]
        public async Task When_Introspecting_AccessToken_Then_Information_Are_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] {"scim"}))
                .ConfigureAwait(false);
            var introspectionClient = await IntrospectClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var introspection = await introspectionClient.Introspect(
                    IntrospectionRequest.Create(result.Content.AccessToken, TokenTypes.AccessToken))
                .ConfigureAwait(false);

            Assert.Single(introspection.Content.Scope);
            Assert.Equal("scim", introspection.Content.Scope.First());
        }

        [Fact]
        public async Task When_Introspecting_RefreshToken_Then_Information_Are_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] {"scim"}))
                .ConfigureAwait(false);
            var introspectionClient = await IntrospectClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var introspection = await introspectionClient.Introspect(
                    IntrospectionRequest.Create(result.Content.AccessToken, TokenTypes.RefreshToken))
                .ConfigureAwait(false);

            Assert.Single(introspection.Content.Scope);
            Assert.Equal("scim", introspection.Content.Scope.First());
        }
    }
}
