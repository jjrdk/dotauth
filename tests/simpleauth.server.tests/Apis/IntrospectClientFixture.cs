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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Client;
    using Client.Operations;
    using Newtonsoft.Json;
    using Shared;
    using Shared.Responses;
    using Xunit;

    public class IntrospectClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public IntrospectClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_No_Parameters_Is_Passed_To_Introspection_Edp_Then_Error_Is_Returned()
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/introspect")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            var error = JsonConvert.DeserializeObject<ErrorResponse>(json);
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("no parameter in body request", error.ErrorDescription);
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
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/introspect")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            var error = JsonConvert.DeserializeObject<ErrorResponse>(json);
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter token is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Introspect_And_Client_Not_Authenticated_Then_Error_Is_Returned()
        {
            var introspection = await new IntrospectClient(
                    TokenCredentials.FromClientCredentials("invalid_client", "invalid_client"),
                    IntrospectionRequest.Create("invalid_token", TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(introspection);
            Assert.True(introspection.ContainsError);
            Assert.Equal("invalid_client", introspection.Error.Error);
            Assert.Equal("the client doesn't exist", introspection.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Introspect_And_Token_Doesnt_Exist_Then_Error_Is_Returned()
        {
            var introspection = await new IntrospectClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    IntrospectionRequest.Create("invalid_token", TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(introspection);
            Assert.True(introspection.ContainsError);
            Assert.Equal("invalid_token", introspection.Error.Error);
            Assert.Equal("the token is not valid", introspection.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Introspecting_AccessToken_Then_Information_Are_Returned()
        {
            var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    TokenRequest.FromPassword("administrator", "password", new[] {"scim"}),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var introspection = await new IntrospectClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    IntrospectionRequest.Create(result.Content.AccessToken, TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(introspection);
            Assert.NotNull(introspection.Content.Scope);
            Assert.True(introspection.Content.Scope.Count() == 1);
            Assert.True(introspection.Content.Scope.First() == "scim");
        }

        [Fact]
        public async Task When_Introspecting_RefreshToken_Then_Information_Are_Returned()
        {
            var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    TokenRequest.FromPassword("administrator", "password", new[] {"scim"}),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var introspection = await new IntrospectClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    IntrospectionRequest.Create(result.Content.AccessToken, TokenTypes.RefreshToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(introspection);
            Assert.NotNull(introspection.Content.Scope);
            Assert.True(introspection.Content.Scope.Count() == 1 && introspection.Content.Scope.First() == "scim");
        }
    }
}
