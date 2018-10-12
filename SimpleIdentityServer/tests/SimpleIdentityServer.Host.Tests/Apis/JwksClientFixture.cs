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

using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Jwt;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class JwksClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private readonly HttpClient _httpClientFactoryStub;
        private IJwksClient _jwksClient;

        public JwksClientFixture(TestOauthServerFixture server)
        {
            _server = server;
            _httpClientFactoryStub = _server.Client;

            _jwksClient = new JwksClient(_httpClientFactoryStub, new GetDiscoveryOperation(_httpClientFactoryStub));
        }

        [Fact]
        public async Task When_Requesting_JWKS_Then_List_Is_Returned()
        {
            // ACT 
           var jwks = await  _jwksClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(jwks);
        }

        [Fact]
        public async Task When_Get_AccessToken_Then_Signature_Is_Correct()
        {
            //_httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var jwsParser = new JwsParserFactory().BuildJwsParser();

            // ACT
            var result = await new TokenClient(_httpClientFactoryStub,null,new GetDiscoveryOperation(_httpClientFactoryStub)) // _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var jwks = await _jwksClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
            var accessToken = result.Content.AccessToken;
            var payload = jwsParser.ValidateSignature(accessToken, jwks);
            Assert.NotNull(payload);
        }

        [Fact]
        public async Task When_Get_Access_Token_And_Rotate_JsonWebKeySet_Then_Signature_Is_Not_Correct()
        {
            // ARRANGE
            var jwsParser = new JwsParserFactory().BuildJwsParser();

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(baseUrl + "/jwks"),
                Method = HttpMethod.Put
            };
            await _server.Client.SendAsync(httpRequestMessage).ConfigureAwait(false);
            var jwks = await _jwksClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
            var accessToken = result.Content.AccessToken;
            var payload = jwsParser.ValidateSignature(accessToken, jwks);
            Assert.Null(payload);
        }
    }
}
