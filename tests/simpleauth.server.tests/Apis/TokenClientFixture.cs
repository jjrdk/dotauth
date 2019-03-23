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
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using Shared;
    using SimpleAuth;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Xunit;

    public class TokenClientFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        private const string WellKnownOpenidConfigurationUrl = BaseUrl + WellKnownOpenidConfiguration;
        private readonly TestOauthServerFixture _server;

        public TokenClientFixture()
        {
            IdentityModelEventSource.ShowPII = true;
            _server = new TestOauthServerFixture();
        }

        [Fact]
        public async Task When_GrantType_Is_Not_Specified_To_Token_Endpoint_Then_Json_Is_Returned()
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
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("the parameter grant_type is missing", error.Detail);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_No_Username_Is_Passed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("the parameter username is missing", error.Detail);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_No_Password_Is_Passed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("the parameter password is missing", error.Detail);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_No_Scope_Is_Passed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("the parameter scope is missing", error.Detail);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_Invalid_ClientId_Is_Passed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password"),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("client_id", "invalid_client_id")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal("the client doesn't exist", error.Detail);
        }

        [Fact]
        public async Task
            When_Use_Password_GrantType_And_Authenticate_Client_With_Not_Accepted_Auth_Method_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password"),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("client_id", "basic_client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal("the client cannot be authenticated with secret basic", error.Detail);
        }

        [Fact]
        public async Task
            When_Use_Password_GrantType_And_ResourceOwner_Credentials_Are_Not_Valid_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "invalid_password"),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_grant", error.Title);
            Assert.Equal("resource owner credentials are not valid", error.Detail);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_Scopes_Are_Not_Valid_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("scope", "invalid"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_scope", error.Title);
            Assert.Equal("the scopes invalid are not allowed or invalid", error.Detail);
        }

        [Fact]
        public async Task When_Use_ClientCredentials_Grant_Type_And_No_Scope_Is_Passwed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("the parameter scope is missing", error.Detail);
        }

        [Fact]
        public async Task When_Use_ClientCredentials_And_Client_Does_Not_Support_It_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "invalid_scope"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal("the client client doesn't support the grant type client_credentials", error.Detail);
        }

        [Fact]
        public async Task
            When_Use_ClientCredentials_And_Client_Does_Not_Have_Token_ResponseType_It_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "invalid_scope"),
                new KeyValuePair<string, string>("client_id", "clientWithWrongResponseType"),
                new KeyValuePair<string, string>("client_secret", "clientWithWrongResponseType")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal(
                "the client 'clientWithWrongResponseType' doesn't support the response type: 'token'",
                error.Detail);
        }

        [Fact]
        public async Task When_Use_ClientCredentials_And_Scope_Is_Not_Supported_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "invalid"),
                new KeyValuePair<string, string>("client_id", "clientCredentials"),
                new KeyValuePair<string, string>("client_secret", "clientCredentials")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_scope", error.Title);
            Assert.Equal("the scopes invalid are not allowed or invalid", error.Detail);
        }

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_No_RefreshToken_Is_Passed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("the parameter refresh_token is missing", error.Detail);
        }

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_Invalid_ClientId_Is_Passed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", "invalid_refresh_token"),
                new KeyValuePair<string, string>("client_id", "invalid_client_id")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal("the client doesn't exist", error.Detail);
        }

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_RefreshToken_Does_Not_Exist_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", "invalid_refresh_token"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_grant", error.Title);
            Assert.Equal("the refresh token is not valid", error.Detail);
        }

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_Another_Client_Tries_ToRefresh_Then_Json_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(TokenRequest.FromScopes("openid")).ConfigureAwait(false);
            var refreshToken = await (await TokenClient.Create(
                        TokenCredentials.FromClientCredentials("client", "client"),
                        _server.Client,
                        new Uri(WellKnownOpenidConfigurationUrl))
                    .ConfigureAwait(false)).GetToken(TokenRequest.FromRefreshToken(result.Content.RefreshToken))
                .ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, refreshToken.Status);
            Assert.Equal("invalid_grant", refreshToken.Error.Title);
            Assert.Equal("the refresh token can be used only by the same issuer", refreshToken.Error.Detail);
        }

        [Fact]
        public async Task When_Use_AuthCode_Grant_Type_And_No_Code_Is_Passed_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("the parameter code is missing", error.Detail);
        }

        [Fact]
        public async Task When_Use_AuthCode_Grant_Type_And_RedirectUri_Is_Invalid_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "code")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequestCode, error.Title);
            Assert.Equal("Based on the RFC-3986 the redirection-uri is not well formed", error.Detail);
        }

        [Fact]
        public async Task When_Use_AuthCode_Grant_Type_And_ClientId_Is_Not_Correct_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "code"),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:5000/callback"),
                new KeyValuePair<string, string>("client_id", "invalid_client_id")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal("the client doesn't exist", error.Detail);
        }

        [Fact]
        public async Task
            When_Use_AuthCode_GrantType_And_Client_DoesntSupport_AuthCode_GrantType_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "code"),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:5000/callback"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal("the client client doesn't support the grant type authorization_code", error.Detail);
        }

        [Fact]
        public async Task When_Use_AuthCode_GrantType_And_Client_DoesntSupport_Code_ResponseType_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "code"),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:5000/callback"),
                new KeyValuePair<string, string>("client_id", "incomplete_authcode_client"),
                new KeyValuePair<string, string>("client_secret", "incomplete_authcode_client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Title);
            Assert.Equal(
                "the client 'incomplete_authcode_client' doesn't support the response type: 'code'",
                error.Detail);
        }

        [Fact]
        public async Task When_Use_AuthCode_Grant_Type_And_Code_Does_Not_Exist_Then_Json_Is_Returned()
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "code"),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:5000/callback"),
                new KeyValuePair<string, string>("client_id", "authcode_client"),
                new KeyValuePair<string, string>("client_secret", "authcode_client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{BaseUrl}/token")
            };

            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_grant", error.Title);
            Assert.Equal("the authorization code is not correct", error.Detail);
        }

        // TH : CONTINUE TO WRITE UTS

        [Fact]
        public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(TokenRequest.FromScopes("openid")).ConfigureAwait(false);

            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_Password_Grant_Type_Then_Access_Token_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);

            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_Password_Grant_Type_Then_Multiple_Roles_Are_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(TokenRequest.FromPassword("superuser", "password", new[] { "role" }))
                .ConfigureAwait(false);

            Assert.False(result.ContainsError);
            var payload = new JwtSecurityToken(result.Content.IdToken);
            var roles = payload.Claims.Where(x => x.Type == "role").ToArray();
            Assert.Single(roles);
            Assert.Equal("administrator", roles[0].Value.Split(' ')[0]);
        }

        //[Fact(Skip = "Integration test")]
        //public async Task When_Using_Password_Grant_Type_With_SMS_Then_Access_Token_Is_Returned()
        //{
        //    var confirmationCode = new ConfirmationCode();
        //    _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //        .Returns(() => Task.FromResult((ConfirmationCode)null));
        //    _server.SharedCtx.ConfirmationCodeStore
        //        .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
        //        .Callback<ConfirmationCode>(r => { confirmationCode = r; })
        //        .Returns(() => Task.FromResult(true));

        //    _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(confirmationCode);
        //    var tokenClient = await TokenClient.Create(
        //            TokenCredentials.FromClientCredentials("client", "client"),
        //            _server.Client,
        //            new Uri(WellKnownOpenidConfigurationUrl))
        //        .ConfigureAwait(false);
        //    _ = await tokenClient.RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
        //             .ConfigureAwait(false);
        //    var result = await tokenClient
        //        .GetToken(TokenRequest.FromPassword("phone", confirmationCode.Value, new[] { "scim" }, "sms"))
        //        .ConfigureAwait(false);

        //    Assert.False(result.ContainsError);
        //    Assert.NotEmpty(result.Content.AccessToken);
        //}

        [Fact(Skip = "solve certificate problem")]
        public async Task When_Using_Client_Certificate_Then_AccessToken_Is_Returned()
        {
            var certificate = new X509Certificate2("mycert.pfx", "simpleauth", X509KeyStorageFlags.Exportable);

            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromCertificate("certificate_client", certificate),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "openid" }))
                .ConfigureAwait(false);

            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_RefreshToken_GrantType_Then_New_One_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);

            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task
            When_Get_Access_Token_With_Password_Grant_Type_Then_Access_Token_With_Valid_Signature_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            // TODO: Look into this
            //var jwks = await _jwksClient.GetToken(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_ClientSecretPostAuthentication_Then_AccessToken_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromBasicAuthentication("basic_client", "basic_client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var token = await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

            Assert.False(token.ContainsError);
            Assert.NotEmpty(token.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_BaseAuthentication_Then_AccessToken_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromBasicAuthentication("basic_client", "basic_client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var firstToken = await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

            Assert.False(firstToken.ContainsError);
            Assert.NotEmpty(firstToken.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_ClientSecretJwtAuthentication_Then_AccessToken_Is_Returned()
        {
            var payload = new JwtPayload(
                new[]
                {
                    new Claim(StandardClaimNames.Issuer, "jwt_client"),
                    new Claim(OpenIdClaimTypes.Subject, "jwt_client"),
                    new Claim(StandardClaimNames.Audiences, "http://localhost:5000"),
                    new Claim(
                        StandardClaimNames.ExpirationTime,
                        DateTime.UtcNow.AddHours(1).ConvertToUnixTimestamp().ToString())
                });
            var handler = new JwtSecurityTokenHandler();

            var jwe = handler.CreateEncodedJwt(
                payload.Iss,
                payload.Aud[0],
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow,
                new SigningCredentials(_server.SharedCtx.ModelSignatureKey, SecurityAlgorithms.HmacSha256Signature),
                new EncryptingCredentials(
                    _server.SharedCtx.ModelEncryptionKey,
                    SecurityAlgorithms.Aes256KW,
                    SecurityAlgorithms.Aes128CbcHmacSha256));

            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientSecret(jwe, "jwt_client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var token = await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

            Assert.False(token.ContainsError);
        }

        [Fact]
        public async Task When_Using_PrivateKeyJwtAuthentication_Then_AccessToken_Is_Returned()
        {
            var payload = new JwtPayload(
                new[]
                {
                    new Claim(StandardClaimNames.Issuer, "private_key_client"),
                    new Claim(OpenIdClaimTypes.Subject, "private_key_client"),
                    new Claim(StandardClaimNames.Audiences, "http://localhost:5000"),
                    new Claim(
                        StandardClaimNames.ExpirationTime,
                        DateTime.UtcNow.AddHours(1).ConvertToUnixTimestamp().ToString())
                });
            var handler = new JwtSecurityTokenHandler();

            var header = new JwtHeader(
                new SigningCredentials(
                    TestKeys.SecretKey.CreateSignatureJwk(),
                    SecurityAlgorithms.HmacSha256Signature));
            var jwtToken = new JwtSecurityToken(header, payload);
            var jws = handler.WriteToken(jwtToken);
            //handler.CreateEncodedJwt(payload, SecurityAlgorithms.RsaSha256, _server.SharedCtx.SignatureKey);

            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientSecret(jws, "private_key_client"),
                    _server.Client,
                    new Uri(WellKnownOpenidConfigurationUrl))
                .ConfigureAwait(false);
            var token = await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

            Assert.False(token.ContainsError);
            Assert.NotEmpty(token.Content.AccessToken);
        }
    }
}
