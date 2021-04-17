// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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
    using Microsoft.IdentityModel.Logging;
    using Newtonsoft.Json;
    using Shared;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Models;
    using Xunit;
    using Xunit.Abstractions;

    public class RevokeTokenClientFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        private readonly TestOauthServerFixture _server;

        public RevokeTokenClientFixture(ITestOutputHelper outputHelper)
        {
            IdentityModelEventSource.ShowPII = true;
            _server = new TestOauthServerFixture(outputHelper);
        }

        [Fact]
        public async Task When_No_Parameters_Is_Passed_To_TokenRevoke_Edp_Then_Error_Is_Returned()
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}/token/revoke")
            };

            var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.MissingParameter, "token"), error.Detail);
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
                RequestUri = new Uri($"{BaseUrl}/token/revoke")
            };

            var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.MissingParameter, "token"), error.Detail);
        }

        [Fact]
        public async Task When_Revoke_Token_And_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("invalid_client", "invalid_client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var ex = await tokenClient.RevokeToken(RevokeTokenRequest.Create("access_token", TokenTypes.AccessToken))
                .ConfigureAwait(false);

            Assert.True(ex.HasError);
            Assert.Equal("invalid_client", ex.Error.Title);
            Assert.Equal(Strings.TheClientDoesntExist, ex.Error.Detail);
        }

        [Fact]
        public async Task When_Token_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var ex = await tokenClient.RevokeToken(RevokeTokenRequest.Create("access_token", TokenTypes.AccessToken))
                .ConfigureAwait(false);

            Assert.True(ex.HasError);
            Assert.Equal("invalid_token", ex.Error.Title);
            Assert.Equal(Strings.TheTokenDoesntExist, ex.Error.Detail);
        }

        [Fact]
        public async Task When_Revoke_Token_And_Client_Is_Different_Then_Error_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var revokeClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var ex = await revokeClient
                .RevokeToken(RevokeTokenRequest.Create(result.Content.AccessToken, TokenTypes.AccessToken))
                .ConfigureAwait(false);

            Assert.True(ex.HasError);
            Assert.Equal("invalid_token", ex.Error.Title);
            Assert.Equal("The token has not been issued for the given client id 'client'", ex.Error.Detail);
        }

        [Fact]
        public async Task When_Revoking_AccessToken_Then_True_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var revoke = await tokenClient
                .RevokeToken(RevokeTokenRequest.Create(result.Content.AccessToken, TokenTypes.AccessToken))
                .ConfigureAwait(false);
            var introspectionClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var ex = await introspectionClient.Introspect(
                    IntrospectionRequest.Create(result.Content.AccessToken, TokenTypes.AccessToken, "pat"))
                .ConfigureAwait(false);

            Assert.False(revoke.HasError);
            Assert.True(ex.HasError);
        }

        [Fact]
        public async Task When_Revoking_RefreshToken_Then_True_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var revoke = await tokenClient
                .RevokeToken(RevokeTokenRequest.Create(result.Content.RefreshToken, TokenTypes.RefreshToken))
                .ConfigureAwait(false);
            var introspectClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var ex = await introspectClient.Introspect(
                    IntrospectionRequest.Create(result.Content.RefreshToken, TokenTypes.RefreshToken, "pat"))
                .ConfigureAwait(false);

            Assert.False(revoke.HasError);
            Assert.True(ex.HasError);
        }
    }
}
