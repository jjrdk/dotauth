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

namespace SimpleAuth.Server.Tests.Apis
{
    using Client;
    using Newtonsoft.Json;
    using Shared.Models;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Xunit;
    using TokenRequest = Client.TokenRequest;

    public class RegisterClientFixture : IDisposable
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string ApplicationJson = "application/json";
        private readonly TestOauthServerFixture _server;
        private readonly RegistrationClient _registrationClient;
        private readonly string _openIdConfigUrl;

        public RegisterClientFixture()
        {
            _server = new TestOauthServerFixture();
            _registrationClient = new RegistrationClient(_server.Client);
            _openIdConfigUrl = $"{BaseUrl}/.well-known/openid-configuration";
        }

        [Fact(Skip = "Run locally")]
        public async Task When_Empty_Json_Request_Is_Passed_To_Registration_Api_Then_Error_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(_openIdConfigUrl))
                .ConfigureAwait(false);
            var grantedToken = await tokenClient.GetToken(TokenRequest.FromScopes("register_client"))
                .ConfigureAwait(false);
            var obj = new { fake = "fake" };
            var fakeJson = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(ApplicationJson);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        }

        [Fact]
        public async Task When_Pass_Invalid_Redirect_Uris_Then_Error_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(_openIdConfigUrl))
                .ConfigureAwait(false);
            var grantedToken = await tokenClient.GetToken(TokenRequest.FromScopes("register_client"))
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { "openid" },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { "localhost" },
                ClientUri = new Uri("http://google.com"),
                TosUri = new Uri("http://google.com"),
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet()
            };
            var fakeJson = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(ApplicationJson);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);

            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, error.Title);
        }

        [Fact(Skip = "Run locally")]
        public async Task When_Pass_Redirect_Uri_With_Fragment_Then_Error_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(_openIdConfigUrl))
                .ConfigureAwait(false);
            var grantedToken = await tokenClient.GetToken(TokenRequest.FromScopes("register_client"))
                .ConfigureAwait(false);
            var obj = new
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                AllowedScopes = new[] { "openid" },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost#fragment") },
                //LogoUri = "http://google.com",
                ClientUri = new Uri("https://valid")
            };
            var fakeJson = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(ApplicationJson);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(grantedToken.Content.TokenType, grantedToken.Content.AccessToken);

            var httpResult = await _server.SharedCtx.Client.SendAsync(httpRequest).ConfigureAwait(false);

            //Assert.Equal(HttpStatusCode.OK, httpResult.StatusCode);

            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal("invalid_redirect_uri", error.Title);
            Assert.Equal(
                string.Format(
                    SimpleAuth.Shared.Errors.ErrorDescriptions.TheRedirectUrlCannotContainsFragment,
                    "http://localhost/#fragment"),
                error.Detail);
        }

        [Fact(Skip = "Run locally")]
        public async Task When_Pass_Invalid_Client_Uri_Then_Error_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(_openIdConfigUrl))
                .ConfigureAwait(false);
            var grantedToken = await tokenClient.GetToken(TokenRequest.FromScopes("register_client"))
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { "openid" },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                LogoUri = "http://google.com",
                ClientUri = "invalid_client_uri"
            };
            var fakeJson = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}/registration"),
                Content = new StringContent(fakeJson, Encoding.UTF8, ApplicationJson)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal("invalid_client_metadata", error.Title);
            Assert.Equal("the parameter client_uri is not correct", error.Detail);
        }

        [Fact(Skip = "Run locally")]
        public async Task When_Pass_Invalid_Tos_Uri_Then_Error_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(_openIdConfigUrl))
                .ConfigureAwait(false);
            var grantedToken = await tokenClient.GetToken(TokenRequest.FromScopes("register_client"))
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { "openid" },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                LogoUri = new Uri("http://google.com"),
                ClientUri = new Uri("https://valid_client_uri"),
                TosUri = "invalid"
            };
            var fakeJson = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(ApplicationJson);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal("invalid_client_metadata", error.Title);
            Assert.Equal("the parameter tos_uri is not correct", error.Detail);
        }

        [Fact(Skip = "Run locally")]
        public async Task When_Registering_A_Client_Then_No_Exception_Is_Thrown()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(_openIdConfigUrl))
                .ConfigureAwait(false);
            var grantedToken = await tokenClient.GetToken(TokenRequest.FromScopes("register_client"))
                .ConfigureAwait(false);

            var client = await _registrationClient.Resolve(
                    new Client
                    {
                        JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                        AllowedScopes = new[] { "openid" },
                        ClientName = "Test",
                        ClientId = "id",
                        RedirectionUrls = new[] { new Uri("https://localhost"), },
                        RequestUris = new[] { new Uri("https://localhost") },
                    },
                    BaseUrl + "/.well-known/openid-configuration",
                    grantedToken.Content.AccessToken)
                .ConfigureAwait(false);

            Assert.NotNull(client.Content);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
