// Copyright 2016 Habart Thierry
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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Shared;
    using Shared.Responses;
    using SimpleIdentityServer.Client;
    using SimpleIdentityServer.Client.Operations;
    using Xunit;

    public class RevokeTokenClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public RevokeTokenClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_No_Parameters_Is_Passed_To_TokenRevoke_Edp_Then_Error_Is_Returned()
        {            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/token/revoke")
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
        {            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("invalid", "invalid")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token/revoke")
            };

                        var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var error = JsonConvert.DeserializeObject<ErrorResponse>(json);
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter token is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Revoke_Token_And_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
        {
                        var ex = await new RevokeTokenClient(
                    TokenCredentials.FromClientCredentials("invalid_client", "invalid_client"),
                    RevokeTokenRequest.RevokeToken("access_token", TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);

                        Assert.True(ex.ContainsError);
            Assert.Equal("invalid_client", ex.Error.Error);
            Assert.Equal("the client doesn't exist", ex.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Token_Doesnt_Exist_Then_Error_Is_Returned()
        {
                        var ex = await new RevokeTokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    RevokeTokenRequest.RevokeToken("access_token", TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

                        Assert.True(ex.ContainsError);
            Assert.Equal("invalid_token", ex.Error.Error);
            Assert.Equal("the token doesn't exist", ex.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Revoke_Token_And_Client_Is_Different_Then_Error_Is_Returned()
        {
                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15"),
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var ex = await new RevokeTokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    RevokeTokenRequest.RevokeToken(result.Content.AccessToken, TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

                        Assert.True(ex.ContainsError);
            Assert.Equal("invalid_token", ex.Error.Error);
            Assert.Equal("the token has not been issued for the given client id 'client'", ex.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Revoking_AccessToken_Then_True_Is_Returned()
        {
                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var revoke = await new RevokeTokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    RevokeTokenRequest.RevokeToken(result.Content.AccessToken, TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var ex = await new IntrospectClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    IntrospectionRequest.Create(result.Content.AccessToken, TokenTypes.AccessToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

                        Assert.False(revoke.ContainsError);
            Assert.True(ex.ContainsError);
        }

        [Fact]
        public async Task When_Revoking_RefreshToken_Then_True_Is_Returned()
        {
                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    TokenRequest.FromPassword("administrator", "password", new []{"scim"}),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var revoke = await new RevokeTokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    RevokeTokenRequest.RevokeToken(result.Content.RefreshToken, TokenTypes.RefreshToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var ex = await new IntrospectClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    IntrospectionRequest.Create(result.Content.RefreshToken, TokenTypes.RefreshToken),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

                        Assert.False(revoke.ContainsError);
            Assert.True(ex.ContainsError);
        }
    }
}
