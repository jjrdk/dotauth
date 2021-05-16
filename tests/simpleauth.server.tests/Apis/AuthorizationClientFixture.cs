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
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using MiddleWares;
    using Newtonsoft.Json;
    using Shared;
    using Shared.Requests;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;
    using Xunit;
    using Xunit.Abstractions;
    using TokenRequest = Client.TokenRequest;

    public class AuthorizationClientFixture : IDisposable
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        private readonly TestOauthServerFixture _server;
        private readonly TokenClient _authorizationClient;
        private readonly JwtSecurityTokenHandler _jwsGenerator = new();

        public AuthorizationClientFixture(ITestOutputHelper outputHelper)
        {
            IdentityModelEventSource.ShowPII = true;
            _server = new TestOauthServerFixture(outputHelper);
            _authorizationClient = new TokenClient(
                TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
        }

        [Fact]
        public async Task When_Scope_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client().GetAsync(new Uri(BaseUrl + "/authorization")).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(HttpStatusCode.BadRequest, error.Status);
            Assert.Equal(string.Format(Strings.MissingParameter, "scope"), error.Detail);
        }

        [Fact]
        public async Task When_ClientId_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(new Uri(BaseUrl + "/authorization?scope=scope"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.MissingParameter, "client_id"), error.Detail);
        }

        [Fact]
        public async Task When_RedirectUri_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(new Uri(BaseUrl + "/authorization?scope=scope&client_id=client"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.MissingParameter, "redirect_uri"), error.Detail);
        }

        [Fact]
        public async Task When_ResponseType_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var redirect = Uri.EscapeUriString("https://redirect_uri");
            var httpResult = await _server.Client()
                .GetAsync(new Uri(BaseUrl + $"/authorization?scope=scope&client_id=client&redirect_uri={redirect}"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.MissingParameter, "response_type"), error.Detail);
        }

        [Fact]
        public async Task When_Unsupported_ResponseType_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var redirect = Uri.EscapeUriString("https://redirect_uri");
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + $"/authorization?scope=scope&state=state&client_id=client&redirect_uri={redirect}&response_type=invalid"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal("at least one response_type parameter is not supported", error.Detail);
        }

        [Fact]
        public async Task When_UnsupportedPrompt_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var redirect = Uri.EscapeUriString("https://redirect_uri");
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + $"/authorization?scope=scope&state=state&client_id=client&redirect_uri={redirect}&response_type=token&prompt=invalid"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal("at least one prompt parameter is not supported", error.Detail);
        }

        [Fact]
        public async Task When_Not_Correct_Redirect_Uri_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var redirect = "redirect_uri";
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + $"/authorization?scope=scope&state=state&client_id=client&redirect_uri={redirect}&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(Strings.TheRedirectionUriIsNotWellFormed, error.Detail);
        }

        [Fact]
        public async Task When_Not_Correct_ClientId_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + "/authorization?scope=scope&state=state&client_id=bad_client&redirect_uri=http://localhost:5000&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.ClientIsNotValid, "bad_client"), error.Detail);
        }

        [Fact]
        public async Task When_Not_Support_Redirect_Uri_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + "/authorization?scope=scope&state=state&client_id=pkce_client&redirect_uri=http://localhost:5000&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.RedirectUrlIsNotValid, "http://localhost:5000/"), error.Detail);
        }

        [Fact]
        public async Task
            When_ClientRequiresPkce_And_No_CodeChallenge_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + "/authorization?scope=scope&state=state&client_id=pkce_client&redirect_uri=http://localhost:5000/callback&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.TheClientRequiresPkce, "pkce_client"), error.Detail);
        }

        [Fact]
        public async Task When_Use_Hybrid_And_Nonce_Parameter_Is_Not_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + "/authorization?scope=scope&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(string.Format(Strings.MissingParameter, "nonce"), error.Detail);
        }

        [Fact]
        public async Task When_Use_Hybrid_And_Pass_Invalid_Scope_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + "/authorization?scope=scope&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none&nonce=nonce"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidScope, error.Title);
            Assert.Equal("The scopes scope are not allowed or invalid", error.Detail);
        }

        [Fact]
        public async Task
            When_Use_Hybrid_And_Dont_Pass_Not_Supported_ResponseTypes_To_Authorization_Then_Json_Is_Returned()
        {
            var httpResult = await _server.Client()
                .GetAsync(
                    new Uri(
                        BaseUrl
                        + "/authorization?scope=openid api1&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none&nonce=nonce"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

            Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
            Assert.Equal(
                "The client 'incomplete_authcode_client' doesn't support the response type: 'id_token,code,token'",
                error.Detail);
        }

        [Fact]
        public async Task When_Requesting_AuthorizationCode_And_RedirectUri_IsNotValid_Then_Error_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "implicit_client",
                        new Uri(baseUrl + "/invalid_callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state"))
                .ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.InvalidRequest, result.Error.Title);
        }

        [Fact]
        public async Task When_User_Is_Not_Authenticated_And_Pass_None_Prompt_Then_Error_Is_Returned()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            UserStore.Instance().IsInactive = true;
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.None })
                .ConfigureAwait(false);
            UserStore.Instance().IsInactive = false;

            Assert.True(result.HasError);
            Assert.Equal("login_required", result.Error.Title);
            Assert.Equal("The user needs to be authenticated", result.Error.Detail);
        }

        [Fact]
        public async Task
            When_User_Is_Authenticated__And_Pass_Prompt_And_No_Consent_Has_Been_Given_Then_Error_Is_Returned()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            UserStore.Instance().Subject = "user";
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.None })
                .ConfigureAwait(false);
            UserStore.Instance().Subject = "administrator";

            Assert.True(result.HasError);
            Assert.Equal("interaction_required", result.Error.Title);
            Assert.Equal("The user needs to give his consent", result.Error.Detail);
        }

        [Fact]
        public async Task When_Pass_Invalid_IdTokenHint_To_Authorization_Then_Error_Is_Returned()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { id_token_hint = "token", prompt = "none" })
                .ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.InvalidRequest, result.Error.Title);
            Assert.Equal(Strings.TheIdTokenHintParameterIsNotAValidToken, result.Error.Detail);
        }

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Audience_Is_Not_Correct_Then_Error_Is_Returned()
        {
            // GENERATE JWS
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            var jws = _jwsGenerator.CreateEncodedJwt(
                new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("sub", "administrator") }),
                    SigningCredentials = new SigningCredentials(
                        TestKeys.SecretKey.CreateSignatureJwk(),
                        SecurityAlgorithms.HmacSha256Signature)
                });

            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { id_token_hint = jws, prompt = "none" })
                .ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Title);
        }

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Subject_Does_Not_Match_Then_Error_Is_Returned()
        {
            // GENERATE JWS
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            var jws = _jwsGenerator.CreateEncodedJwt(
                new SecurityTokenDescriptor
                {
                    Audience = "http://localhost:5000",
                    Subject = new ClaimsIdentity(new[] { new Claim("sub", "adm") }),
                    SigningCredentials = new SigningCredentials(
                        TestKeys.SecretKey.CreateSignatureJwk(),
                        SecurityAlgorithms.HmacSha256)
                });
            //var jws = _jwsGenerator.Generate(payload, SecurityAlgorithms.RsaSha256, _server.SharedCtx.SignatureKey);

            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { id_token_hint = jws, prompt = "none" })
                .ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.InvalidRequest, result.Error.Title);
            Assert.Equal(Strings.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken, result.Error.Detail);
        }

        [Fact]
        public async Task When_Requesting_AuthorizationCode_Then_Code_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.None })
                .ConfigureAwait(false);
            var location = result.Content;
            var queries = QueryHelpers.ParseQuery(location.Query);
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("authcode_client", "authcode_client"),
                _server.Client,
                new Uri(baseUrl + WellKnownOpenidConfiguration));
            var token = await tokenClient
                .GetToken(TokenRequest.FromAuthorizationCode(queries["code"], "http://localhost:5000/callback"))
                .ConfigureAwait(false);

            Assert.NotEmpty(token.Content.AccessToken);
            Assert.True(queries["state"] == "state");
        }

        [Fact]
        public async Task When_Pass_MaxAge_And_User_Session_Is_Inactive_Then_Redirect_To_Authenticate_Page()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            UserStore.Instance().AuthenticationOffset = DateTimeOffset.UtcNow.AddDays(-2);
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.None, max_age = 300 })
                .ConfigureAwait(false);
            var location = result.Content;
            UserStore.Instance().AuthenticationOffset = null;

            Assert.Equal("/pwd/Authenticate/OpenId", location.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Login_Prompt_Then_Redirect_To_Authenticate_Page()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.Login })
                .ConfigureAwait(false);

            Assert.Equal("/pwd/Authenticate/OpenId", result.Content.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Consent_Prompt_And_User_Is_Not_Authenticated_Then_Redirect_To_Authenticate_Page()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            UserStore.Instance().IsInactive = true;
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.Consent })
                .ConfigureAwait(false);
            UserStore.Instance().IsInactive = false;

            Assert.Equal("/pwd/Authenticate/OpenId", result.Content.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Consent_Prompt_And_User_Is_Authenticated_Then_Redirect_To_Authenticate_Page()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.Consent })
                .ConfigureAwait(false);

            Assert.Equal("/Consent", result.Content.LocalPath);
        }

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Subject_Matches_The_Authenticated_User_Then_Token_Is_Returned()
        {
            var jwe = _jwsGenerator.CreateEncodedJwt(
                new SecurityTokenDescriptor
                {
                    Audience = "http://localhost:5000",
                    Subject = new ClaimsIdentity(new[] { new Claim("sub", "administrator") }),
                    SigningCredentials =
                        new SigningCredentials(
                            TestKeys.SecretKey.CreateSignatureJwk(),
                            SecurityAlgorithms.HmacSha256),
                    EncryptingCredentials = new EncryptingCredentials(
                        TestKeys.SuperSecretKey.CreateEncryptionJwk(),
                        SecurityAlgorithms.Aes256KW,
                        SecurityAlgorithms.Aes128CbcHmacSha256)
                });

            var pkce = CodeChallengeMethods.S256.BuildPkce();
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "authcode_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { id_token_hint = jwe, prompt = "none" })
                .ConfigureAwait(false);

            Assert.False(result.HasError);
        }

        [Fact]
        public async Task When_Requesting_Token_And_CodeVerifier_Is_Passed_Then_Token_Is_Returned()
        {
            var pkce = CodeChallengeMethods.S256.BuildPkce();

            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.Code },
                        "pkce_client",
                        new Uri(BaseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    {
                        prompt = PromptNames.None
                    })
                .ConfigureAwait(false);
            var location = result.Content;
            var queries = QueryHelpers.ParseQuery(location.Query);
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("pkce_client", "pkce_client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var token = await tokenClient.GetToken(
                    TokenRequest.FromAuthorizationCode(
                        queries["code"],
                        "http://localhost:5000/callback",
                        pkce.CodeVerifier))
                .ConfigureAwait(false);

            Assert.NotNull(token?.Content?.AccessToken);
        }

        [Fact]
        public async Task When_Requesting_IdTokenAndAccessToken_Then_Tokens_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";

            var pkce = CodeChallengeMethods.S256.BuildPkce();
            // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token },
                        "implicit_client",
                        new Uri(baseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.None, nonce = "nonce" })
                .ConfigureAwait(false);
            var queries = QueryHelpers.ParseQuery(result.Content.Fragment.TrimStart('#'));

            Assert.NotNull(result.Content);
            Assert.True(queries.ContainsKey("id_token"));
            Assert.True(queries.ContainsKey("access_token"));
            Assert.True(queries.ContainsKey("state"));
            Assert.True(queries["state"] == "state");
        }

        [Fact]
        public async Task When_RequestingIdTokenAndAuthorizationCodeAndAccessToken_Then_Tokens_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";

            var pkce = CodeChallengeMethods.S256.BuildPkce();
            // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.GetAuthorization(
                    new AuthorizationRequest(
                        new[] { "openid", "api1" },
                        new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token, ResponseTypeNames.Code },
                        "hybrid_client",
                        new Uri(baseUrl + "/callback"),
                        pkce.CodeChallenge,
                        CodeChallengeMethods.S256,
                        "state")
                    { prompt = PromptNames.None, nonce = "nonce" })
                .ConfigureAwait(false);
            var queries = QueryHelpers.ParseQuery(result.Content.Fragment.TrimStart('#'));

            Assert.NotNull(result.Content);
            Assert.True(queries.ContainsKey("id_token"));
            Assert.True(queries.ContainsKey("access_token"));
            Assert.True(queries.ContainsKey("code"));
            Assert.True(queries.ContainsKey("state"));
            Assert.Equal("state", queries["state"]);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _server?.Dispose();
        }
    }
}
