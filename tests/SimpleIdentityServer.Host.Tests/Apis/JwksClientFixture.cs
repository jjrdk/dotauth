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

namespace SimpleIdentityServer.Host.Tests.Apis
{
    using Client;
    using Client.Operations;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth;
    using SimpleAuth.Api.Discovery;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Models;
    using Xunit;

    public class JwksClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private readonly HttpClient _httpClientFactoryStub;
        private readonly IJwksClient _jwksClient;

        public JwksClientFixture(TestOauthServerFixture server)
        {
            _server = server;
            _httpClientFactoryStub = _server.Client;

            _jwksClient = new JwksClient(_httpClientFactoryStub,
                new DiscoveryActions(new DefaultScopeRepository(new List<Scope>()),
                    new DefaultClaimRepository(new List<ClaimAggregate>())));
        }

        [Fact]
        public async Task When_Requesting_JWKS_Then_List_Is_Returned()
        {
                        var jwks = await _jwksClient.ResolveAsync(new Uri(baseUrl + "/.well-known/openid-configuration"))
                .ConfigureAwait(false);

                        Assert.NotNull(jwks);
        }

        [Fact]
        public async Task When_Get_AccessToken_Then_Signature_Is_Correct()
        {
            //_httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var jwsParser = new JwsParserFactory().BuildJwsParser();

                        var result =
                await new TokenClient(
                        TokenCredentials.FromClientCredentials("client", "client"),
                        TokenRequest.FromPassword("administrator", "password", new[] { "scim" }),
                        _httpClientFactoryStub,
                        new GetDiscoveryOperation(
                            _httpClientFactoryStub)) // _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                    .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                    .ConfigureAwait(false);
            var jwks = await _jwksClient.ResolveAsync(new Uri(baseUrl + "/.well-known/openid-configuration"))
                .ConfigureAwait(false);

                        Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
            var accessToken = result.Content.AccessToken;
            var payload = jwsParser.ValidateSignature(accessToken, jwks);
            Assert.NotNull(payload);
        }

        [Fact]
        public async Task When_Get_Access_Token_And_Rotate_JsonWebKeySet_Then_Signature_Is_Not_Correct()
        {            var jwsParser = new JwsParserFactory().BuildJwsParser();

                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }),
                    _httpClientFactoryStub,
                    new GetDiscoveryOperation(
                        _httpClientFactoryStub))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(baseUrl + "/jwks"),
                Method = HttpMethod.Put
            };
            await _server.Client.SendAsync(httpRequestMessage).ConfigureAwait(false);
            var jwks = await _jwksClient.ResolveAsync(new Uri(baseUrl + "/.well-known/openid-configuration"))
                .ConfigureAwait(false);

                        Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
            var accessToken = result.Content.AccessToken;
            var payload = jwsParser.ValidateSignature(accessToken, jwks);
            Assert.Null(payload);
        }
    }
}
