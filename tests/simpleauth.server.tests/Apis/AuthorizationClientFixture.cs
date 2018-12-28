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
    using Encrypt;
    using Errors;
    using Microsoft.Extensions.DependencyInjection;
    using MiddleWares;
    using Newtonsoft.Json;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;
    using Signature;
    using SimpleAuth;
    using System;
    using System.Threading.Tasks;
    using Client;
    using Client.Builders;
    using Client.Operations;
    using Xunit;
    using TokenRequest = Client.TokenRequest;

    public class AuthorizationClientFixture : IDisposable
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private IAuthorizationClient _authorizationClient;
        private IJwsGenerator _jwsGenerator;
        private IJweGenerator _jweGenerator;

        public AuthorizationClientFixture()
        {
            _server = new TestOauthServerFixture();
        }

        [Fact]
        public async Task When_Scope_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization")).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter scope is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_ClientId_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter client_id is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_RedirectUri_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl + "/authorization?scope=scope&client_id=client"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter redirect_uri is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_ResponseType_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var redirect = Uri.EscapeUriString("https://redirect_uri");
            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl + $"/authorization?scope=scope&client_id=client&redirect_uri={redirect}"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter response_type is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Unsupported_ResponseType_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var redirect = Uri.EscapeUriString("https://redirect_uri");
            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  $"/authorization?scope=scope&state=state&client_id=client&redirect_uri={redirect}&response_type=invalid"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("at least one response_type parameter is not supported", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_UnsupportedPrompt_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();
            var redirect = Uri.EscapeUriString("https://redirect_uri");
            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  $"/authorization?scope=scope&state=state&client_id=client&redirect_uri={redirect}&response_type=token&prompt=invalid"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("at least one prompt parameter is not supported", error.ErrorDescription);
            Assert.Equal("state", error.State);

        }

        [Fact]
        public async Task When_Not_Correct_Redirect_Uri_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();
            var redirect = "redirect_uri"; //Uri.EscapeUriString("https://redirect_uri");
            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  $"/authorization?scope=scope&state=state&client_id=client&redirect_uri={redirect}&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, error.Error);
            Assert.Equal(ErrorDescriptions.TheRedirectionUriIsNotWellFormed, error.ErrorDescription);
            Assert.Null(error.State);
        }

        [Fact]
        public async Task When_Not_Correct_ClientId_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();
            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  "/authorization?scope=scope&state=state&client_id=bad_client&redirect_uri=http://localhost:5000&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the client id parameter bad_client doesn't exist or is not valid", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Not_Support_Redirect_Uri_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  "/authorization?scope=scope&state=state&client_id=pkce_client&redirect_uri=http://localhost:5000&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the redirect url http://localhost:5000/ doesn't exist or is not valid",
                error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task
            When_ClientRequiresPkce_And_No_CodeChallenge_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  "/authorization?scope=scope&state=state&client_id=pkce_client&redirect_uri=http://localhost:5000/callback&response_type=token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the client pkce_client requires PKCE", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Use_Hybrid_And_Nonce_Parameter_Is_Not_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  "/authorization?scope=scope&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter nonce is missing", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Use_Hybrid_And_Pass_Invalid_Scope_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  "/authorization?scope=scope&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none&nonce=nonce"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_scope", error.Error);
            Assert.Equal("the scopes scope are not allowed or invalid", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task
            When_Use_Hybrid_And_Dont_Pass_Not_Supported_ResponseTypes_To_Authorization_Then_Json_Is_Returned()
        {
            InitializeFakeObjects();

            var httpResult = await _server.Client
                .GetAsync(new Uri(baseUrl +
                                  "/authorization?scope=openid api1&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none&nonce=nonce"))
                .ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal(
                "the client 'incomplete_authcode_client' doesn't support the response type: 'id_token,code,token'",
                error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Requesting_AuthorizationCode_And_RedirectUri_IsNotValid_Then_Error_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            InitializeFakeObjects();

            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "implicit_client",
                        new Uri(baseUrl + "/invalid_callback"),
                        "state"))
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.True(result.Error.Error == "invalid_request");
        }

        [Fact]
        public async Task When_User_Is_Not_Authenticated_And_Pass_None_Prompt_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            UserStore.Instance().IsInactive = true;
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.None
                    })
                .ConfigureAwait(false);
            UserStore.Instance().IsInactive = false;

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("login_required", result.Error.Error);
            Assert.Equal("the user needs to be authenticated", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task
            When_User_Is_Authenticated__And_Pass_Prompt_And_No_Consent_Has_Been_Given_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            UserStore.Instance().Subject = "user";
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.None
                    })
                .ConfigureAwait(false);
            UserStore.Instance().Subject = "administrator";

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("interaction_required", result.Error.Error);
            Assert.Equal("the user needs to give his consent", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_IdTokenHint_To_Authorization_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        IdTokenHint = "token",
                        Prompt = "none"
                    })
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the id_token_hint parameter is not a valid token", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Audience_Is_Not_Correct_Then_Error_Is_Returned()
        {
            // GENERATE JWS
            InitializeFakeObjects();

            var payload = new JwsPayload
            {
                {
                    "sub", "administrator"
                }
            };
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        IdTokenHint = jws,
                        Prompt = "none"
                    })
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the identity token doesnt contain simple identity server in the audience",
                result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Subject_Doesnt_Match_Then_Error_Is_Returned()
        {
            // GENERATE JWS
            InitializeFakeObjects();

            var payload = new JwsPayload
            {
                {
                    "sub", "adm"
                }
            };
            payload.Add("aud", new[] {"http://localhost:5000"});
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        IdTokenHint = jws,
                        Prompt = "none"
                    })
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the current authenticated user doesn't match with the identity token",
                result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Requesting_AuthorizationCode_Then_Code_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            InitializeFakeObjects();

            // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.None
                    })
                .ConfigureAwait(false);
            var location = result.Location;
            var queries = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(location.Query);
            var token = await new TokenClient(
                    TokenCredentials.FromClientCredentials("authcode_client", "authcode_client"),
                    TokenRequest.FromAuthorizationCode(queries["code"], "http://localhost:5000/callback"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.NotNull(result.Location);
            Assert.NotNull(token);
            Assert.NotEmpty(token.Content.AccessToken);
            Assert.True(queries["state"] == "state");
        }

        [Fact]
        public async Task When_Pass_MaxAge_And_User_Session_Is_Inactive_Then_Redirect_To_Authenticate_Page()
        {
            InitializeFakeObjects();

            UserStore.Instance().AuthenticationOffset = DateTimeOffset.UtcNow.AddDays(-2);
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.None,
                        MaxAge = 300
                    })
                .ConfigureAwait(false);
            var location = result.Location;
            UserStore.Instance().AuthenticationOffset = null;

            Assert.Equal("/pwd/Authenticate/OpenId", location.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Login_Prompt_Then_Redirect_To_Authenticate_Page()
        {
            InitializeFakeObjects();

            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.Login
                    })
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("/pwd/Authenticate/OpenId", result.Location.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Consent_Prompt_And_User_Is_Not_Authenticated_Then_Redirect_To_Authenticate_Page()
        {
            InitializeFakeObjects();

            UserStore.Instance().IsInactive = true;
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.Consent
                    })
                .ConfigureAwait(false);
            UserStore.Instance().IsInactive = false;

            Assert.NotNull(result);
            Assert.Equal("/pwd/Authenticate/OpenId", result.Location.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Consent_Prompt_And_User_Is_Authenticated_Then_Redirect_To_Authenticate_Page()
        {
            InitializeFakeObjects();

            var result = await _authorizationClient.ResolveAsync(
                    baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.Consent
                    })
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("/Consent", result.Location.LocalPath);
        }

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Subject_Matches_The_Authenticated_User_Then_Token_Is_Returned()
        {
            // GENERATE JWS
            InitializeFakeObjects();

            var payload = new JwsPayload
            {
                {
                    "sub", "administrator"
                }
            };
            payload.Add("aud", new[] {"http://localhost:5000"});
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);
            var jwe = _jweGenerator.GenerateJwe(jws,
                JweAlg.RSA1_5,
                JweEnc.A128CBC_HS256,
                _server.SharedCtx.EncryptionKey);

            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "authcode_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        IdTokenHint = jwe,
                        Prompt = "none"
                    })
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.ContainsError);
        }

        [Fact]
        public async Task When_Requesting_Token_And_CodeVerifier_Is_Passed_Then_Token_Is_Returned()
        {
            InitializeFakeObjects();

            var builder = new PkceBuilder();
            var pkce = builder.Build(CodeChallengeMethods.S256);

            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(
                        new[] {"openid", "api1"},
                        new[] {ResponseTypes.Code},
                        "pkce_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        CodeChallenge = pkce.CodeChallenge,
                        CodeChallengeMethod = CodeChallengeMethods.S256,
                        Prompt = PromptNames.None
                    })
                .ConfigureAwait(false);
            var location = result.Location;
            var queries = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(location.Query);
            var token = await new TokenClient(
                    TokenCredentials.FromClientCredentials("pkce_client", "pkce_client"),
                    TokenRequest.FromAuthorizationCode(queries["code"],
                        "http://localhost:5000/callback",
                        pkce.CodeVerifier),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(token);
            Assert.NotEmpty(token.Content.AccessToken);
        }

        [Fact]
        public async Task When_Requesting_IdTokenAndAccessToken_Then_Tokens_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            InitializeFakeObjects(); // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.IdToken, ResponseTypes.Token},
                        "implicit_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.None,
                        Nonce = "nonce"
                    })
                .ConfigureAwait(false);
            var queries =
                Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(result.Location.Fragment.TrimStart('#'));

            Assert.NotNull(result);
            Assert.NotNull(result.Location);
            Assert.True(queries.ContainsKey("id_token"));
            Assert.True(queries.ContainsKey("access_token"));
            Assert.True(queries.ContainsKey("state"));
            Assert.True(queries["state"] == "state");
        }

        [Fact]
        public async Task When_RequestingIdTokenAndAuthorizationCodeAndAccessToken_Then_Tokens_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            InitializeFakeObjects(); // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                    new AuthorizationRequest(new[] {"openid", "api1"},
                        new[] {ResponseTypes.IdToken, ResponseTypes.Token, ResponseTypes.Code},
                        "hybrid_client",
                        new Uri(baseUrl + "/callback"),
                        "state")
                    {
                        Prompt = PromptNames.None,
                        Nonce = "nonce"
                    })
                .ConfigureAwait(false);
            var queries =
                Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(result.Location.Fragment.TrimStart('#'));

            Assert.NotNull(result.Location);
            Assert.True(queries.ContainsKey("id_token"));
            Assert.True(queries.ContainsKey("access_token"));
            Assert.True(queries.ContainsKey("code"));
            Assert.True(queries.ContainsKey("state"));
            Assert.True(queries["state"] == "state");
        }

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _jwsGenerator = provider.GetService<IJwsGenerator>();
            _jweGenerator = provider.GetService<IJweGenerator>();
            var getDiscoveryOperation = new GetDiscoveryOperation(_server.Client);
            _authorizationClient = new AuthorizationClient(_server.Client, getDiscoveryOperation);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
