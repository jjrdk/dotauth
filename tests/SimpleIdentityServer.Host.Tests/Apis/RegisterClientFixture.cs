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
    using Core.Errors;
    using Newtonsoft.Json;
    using Shared;
    using Shared.Models;
    using Shared.Responses;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;
    using TokenRequest = Client.TokenRequest;

    public class RegisterClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private IRegistrationClient _registrationClient;

        public RegisterClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Empty_Json_Request_Is_Passed_To_Registration_Api_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var obj = new { fake = "fake" };
            var fakeJson = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            //Assert.Equal("invalid_redirect_uri", error.Error);
            //Assert.Equal("the parameter request_uris is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Redirect_Uris_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { "localhost" },
                LogoUri = "https://logo",
                ClientUri = new Uri("http://google.com"),
                TosUri = new Uri("http://google.com"),
                JwksUri = "https://invalid_jwks_uri",
                JsonWebKeys = new[] { new JsonWebKey { } }
            };
            var fakeJson = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, error.Error);
        }

        [Fact]
        public async Task When_Pass_Redirect_Uri_With_Fragment_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost#fragment") },
                LogoUri = "http://google.com",
                ClientUri = "https://valid"
            };
            var fakeJson = JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal("invalid_redirect_uri", error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheRedirectUrlCannotContainsFragment,
                    "http://localhost/#fragment"),
                error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Logo_Uri_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                LogoUri = "logo",
                ClientUri = new Uri("http://google.com"),
                TosUri = new Uri("http://google.com"),
                JwksUri = "https://invalid_jwks_uri"
            };
            var fakeJson = JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter logo_uri is not correct", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Client_Uri_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                LogoUri = "http://google.com",
                ClientUri = "invalid_client_uri"
            };
            var fakeJson = JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter client_uri is not correct", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Tos_Uri_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                LogoUri = new Uri("http://google.com"),
                ClientUri = new Uri("https://valid_client_uri"),
                TosUri = "invalid"
            };
            var fakeJson = JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter tos_uri is not correct", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Jwks_Uri_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var obj = new
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                LogoUri = "http://google.com",
                ClientUri = new Uri("http://google.com"),
                TosUri = new Uri("http://google.com"),
                JwksUri = "invalid_jwks_uri"
            };
            var fakeJson = JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter jwks_uri is not correct", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Registering_A_Client_Then_No_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("register_client"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);

            var client = await _registrationClient.ResolveAsync(
                    new Client
                    {
                        AllowedScopes = new[] {new Scope {Name = "openid"}},
                        ClientName = "Test",
                        ClientId = "id",
                        RedirectionUrls = new[]
                        {
                            new Uri("https://localhost"),
                        },
                        RequestUris = new[] {new Uri("https://localhost")},
                        ScimProfile = true
                    },
                    baseUrl + "/.well-known/openid-configuration",
                    grantedToken.Content.AccessToken)
                .ConfigureAwait(false);

            Assert.NotNull(client);
            Assert.True(client.Content.ScimProfile);
        }

        private void InitializeFakeObjects()
        {
            _registrationClient = new RegistrationClient(_server.Client, new GetDiscoveryOperation(_server.Client));
        }
    }
}
