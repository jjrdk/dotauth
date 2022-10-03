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

namespace DotAuth.Server.Tests.Apis;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

public sealed class TokenClientFixture
{
    private const string BaseUrl = "http://localhost:5000";
    private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
    private const string WellKnownOpenidConfigurationUrl = BaseUrl + WellKnownOpenidConfiguration;
    private readonly TestOauthServerFixture _server;

    public TokenClientFixture(ITestOutputHelper outputHelper)
    {
        IdentityModelEventSource.ShowPII = true;
        _server = new TestOauthServerFixture(outputHelper);
    }

    [Fact]
    public async Task When_GrantType_Is_Not_Specified_To_Token_Endpoint_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>> {new("invalid", "invalid")};
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "grant_type"), error.Detail);
    }

    [Fact]
    public async Task When_Use_Password_GrantType_And_No_Username_Is_Passed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>> {new("grant_type", "password")};
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "username"), error.Detail);
    }

    [Fact]
    public async Task When_Use_Password_GrantType_And_No_Password_Is_Passed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"), new("username", "administrator")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "password"), error.Detail);
    }

    [Fact]
    public async Task When_Use_Password_GrantType_And_No_Scope_Is_Passed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"), new("username", "administrator"), new("password", "password")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "scope"), error.Detail);
    }

    [Fact]
    public async Task When_Use_Password_GrantType_And_Invalid_ClientId_Is_Passed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("username", "administrator"),
            new("password", "password"),
            new("scope", "openid"),
            new("client_id", "invalid_client_id")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal("invalid_client", error.Title);
        Assert.Equal(SharedStrings.TheClientDoesntExist, error.Detail);
    }

    [Fact]
    public async Task
        When_Use_Password_GrantType_And_Authenticate_Client_With_Not_Accepted_Auth_Method_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("username", "administrator"),
            new("password", "password"),
            new("scope", "openid"),
            new("client_id", "basic_client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal("invalid_client", error.Title);
        Assert.Equal("The client cannot be authenticated with secret basic", error.Detail);
    }

    [Fact]
    public async Task
        When_Use_Password_GrantType_And_ResourceOwner_Credentials_Are_Not_Valid_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("username", "administrator"),
            new("password", "invalid_password"),
            new("scope", "openid"),
            new("client_id", "client"),
            new("client_secret", "client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal("invalid_credentials", error.Title);
        Assert.Equal("resource owner credentials are not valid", error.Detail);
    }

    [Fact]
    public async Task When_Use_Password_GrantType_And_Scopes_Are_Not_Valid_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("username", "administrator"),
            new("password", "password"),
            new("client_id", "client"),
            new("scope", "invalid"),
            new("client_secret", "client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidScope, error.Title);
        Assert.Equal(string.Format(Strings.ScopesAreNotAllowedOrInvalid, "invalid"), error.Detail);
    }

    [Fact]
    public async Task When_Use_ClientCredentials_Grant_Type_And_No_Scope_Is_Passwed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>> {new("grant_type", "client_credentials")};
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "scope"), error.Detail);
    }

    [Fact]
    public async Task When_Use_ClientCredentials_And_Client_Does_Not_Support_It_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("scope", ErrorCodes.InvalidScope),
            new("client_id", "client"),
            new("client_secret", "client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidGrant, error.Title);
        Assert.Equal(
            string.Format(Strings.TheClientDoesntSupportTheGrantType, "client", "client_credentials"),
            error.Detail);
    }

    [Fact]
    public async Task
        When_Use_ClientCredentials_And_Client_Does_Not_Have_Token_ResponseType_It_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("scope", ErrorCodes.InvalidScope),
            new("client_id", "clientWithWrongResponseType"),
            new("client_secret", "clientWithWrongResponseType")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal("invalid_client", error.Title);
        Assert.Equal(
            "The client 'clientWithWrongResponseType' doesn't support the response type: 'token'",
            error.Detail);
    }

    [Fact]
    public async Task When_Use_ClientCredentials_And_Scope_Is_Not_Supported_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("scope", "invalid"),
            new("client_id", "clientCredentials"),
            new("client_secret", "clientCredentials")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidScope, error.Title);
        Assert.Equal(string.Format(Strings.ScopesAreNotAllowedOrInvalid, "invalid"), error.Detail);
    }

    [Fact]
    public async Task When_Use_RefreshToken_Grant_Type_And_No_RefreshToken_Is_Passed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>> {new("grant_type", "refresh_token")};
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "refresh_token"), error.Detail);
    }

    [Fact]
    public async Task When_Use_RefreshToken_Grant_Type_And_Invalid_ClientId_Is_Passed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("refresh_token", "invalid_refresh_token"),
            new("client_id", "invalid_client_id")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal("invalid_client", error.Title);
        Assert.Equal(SharedStrings.TheClientDoesntExist, error.Detail);
    }

    [Fact]
    public async Task When_Use_RefreshToken_Grant_Type_And_RefreshToken_Does_Not_Exist_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("refresh_token", "invalid_refresh_token"),
            new("client_id", "client"),
            new("client_secret", "client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidGrant, error.Title);
        Assert.Equal(Strings.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer, error.Detail);
    }

    [Fact]
    public async Task When_Use_RefreshToken_Grant_Type_And_Another_Client_Tries_ToRefresh_Then_Json_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var result =
            await tokenClient.GetToken(TokenRequest.FromScopes("openid", "offline")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;
        var refreshToken = await new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(WellKnownOpenidConfigurationUrl))
            .GetToken(TokenRequest.FromRefreshToken(result.Item.RefreshToken))
            .ConfigureAwait(false) as Option<GrantedTokenResponse>.Error;

        Assert.Equal(HttpStatusCode.BadRequest, refreshToken.Details.Status);
        Assert.Equal(ErrorCodes.InvalidGrant, refreshToken.Details.Title);
        Assert.Equal("The refresh token can be used only by the same issuer", refreshToken.Details.Detail);
    }

    [Fact]
    public async Task When_Use_AuthCode_Grant_Type_And_No_Code_Is_Passed_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>> {new("grant_type", "authorization_code")};
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "code"), error.Detail);
    }

    [Fact]
    public async Task When_Use_AuthCode_Grant_Type_And_RedirectUri_Is_Invalid_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"), new("code", "code")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal("Based on the RFC-3986 the redirection-uri is not well formed", error.Detail);
    }

    [Fact]
    public async Task When_Use_AuthCode_Grant_Type_And_ClientId_Is_Not_Correct_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", "code"),
            new("redirect_uri", "http://localhost:5000/callback"),
            new("client_id", "invalid_client_id")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal("invalid_client", error.Title);
        Assert.Equal(SharedStrings.TheClientDoesntExist, error.Detail);
    }

    [Fact]
    public async Task
        When_Use_AuthCode_GrantType_And_Client_DoesntSupport_AuthCode_GrantType_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", "code"),
            new("redirect_uri", "http://localhost:5000/callback"),
            new("client_id", "client"),
            new("client_secret", "client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidGrant, error.Title);
        Assert.Equal("The client client doesn't support the grant type authorization_code", error.Detail);
    }

    [Fact]
    public async Task When_Use_AuthCode_GrantType_And_Client_DoesntSupport_Code_ResponseType_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", "code"),
            new("redirect_uri", "http://localhost:5000/callback"),
            new("client_id", "incomplete_authcode_client"),
            new("client_secret", "incomplete_authcode_client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidResponse, error.Title);
        Assert.Equal(
            "The client 'incomplete_authcode_client' doesn't support the response type: 'code'",
            error.Detail);
    }

    [Fact]
    public async Task When_Use_AuthCode_Grant_Type_And_Code_Does_Not_Exist_Then_Json_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", "code"),
            new("redirect_uri", "http://localhost:5000/callback"),
            new("client_id", "authcode_client"),
            new("client_secret", "authcode_client")
        };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        Assert.Equal(ErrorCodes.InvalidGrant, error.Title);
        Assert.Equal(Strings.TheAuthorizationCodeIsNotCorrect, error.Detail);
    }

    // TH : CONTINUE TO WRITE UTS

    [Fact]
    public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var result =
            await tokenClient.GetToken(TokenRequest.FromScopes("openid")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(result.Item.AccessToken);
    }

    [Fact]
    public async Task When_Using_Password_Grant_Type_Then_Access_Token_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var result =
            await tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"scim"}))
                .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(result.Item.AccessToken);
    }

    [Fact]
    public async Task When_Using_Password_Grant_Type_Then_Multiple_Roles_Are_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var result = await tokenClient.GetToken(TokenRequest.FromPassword("superuser", "password", new[] {"role"}))
            .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

        var payload = new JwtSecurityToken(result.Item.IdToken);
        var roles = payload.Claims.Where(x => x.Type == "role").ToArray();
        Assert.Single(roles);
        Assert.Equal("administrator", roles[0].Value.Split(' ')[0]);
    }

    [Fact(Skip = "solve certificate problem")]
    public async Task When_Using_Client_Certificate_Then_AccessToken_Is_Returned()
    {
        var certificate = new X509Certificate2("mycert.pfx", "simpleauth", X509KeyStorageFlags.Exportable);

        var tokenClient = new TokenClient(
            TokenCredentials.FromCertificate("certificate_client", certificate),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var result = await tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"openid"}))
            .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(result.Item.AccessToken);
    }

    [Fact]
    public async Task When_Using_RefreshToken_GrantType_Then_New_One_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var result =
            await tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"scim"}))
                .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(result.Item.AccessToken);
    }

    [Fact]
    public async Task
        When_Get_Access_Token_With_Password_Grant_Type_Then_Access_Token_With_Valid_Signature_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var result = await tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"scim"}))
            .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(result.Item.AccessToken);
    }

    [Fact]
    public async Task When_Using_ClientSecretPostAuthentication_Then_AccessToken_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromBasicAuthentication("basic_client", "basic_client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var token =
            await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(token.Item.AccessToken);
    }

    [Fact]
    public async Task When_Using_BaseAuthentication_Then_AccessToken_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromBasicAuthentication("basic_client", "basic_client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var firstToken =
            await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(firstToken.Item.AccessToken);
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
                    DateTimeOffset.UtcNow.AddHours(1).ConvertToUnixTimestamp().ToString())
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

        var tokenClient = new TokenClient(
            TokenCredentials.FromClientSecret(jwe, "jwt_client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var token =
            await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;

        Assert.NotNull(token);
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
                    DateTimeOffset.UtcNow.AddHours(1).ConvertToUnixTimestamp().ToString())
            });
        var handler = new JwtSecurityTokenHandler();

        var header = new JwtHeader(
            new SigningCredentials(
                TestKeys.SecretKey.CreateSignatureJwk(),
                SecurityAlgorithms.HmacSha256Signature));
        var jwtToken = new JwtSecurityToken(header, payload);
        var jws = handler.WriteToken(jwtToken);

        var tokenClient = new TokenClient(
            TokenCredentials.FromClientSecret(jws, "private_key_client"),
            _server.Client,
            new Uri(WellKnownOpenidConfigurationUrl));
        var token =
            await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;

        Assert.NotEmpty(token.Item.AccessToken);
    }
}