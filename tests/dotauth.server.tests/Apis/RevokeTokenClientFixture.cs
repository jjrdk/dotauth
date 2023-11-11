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

namespace DotAuth.Server.Tests.Apis;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

public sealed class RevokeTokenClientFixture
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
            Method = HttpMethod.Post, RequestUri = new Uri($"{BaseUrl}/token/revoke")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest);
        var json = await httpResult.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json)!;

        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "token"), error.Detail);
    }

    [Fact]
    public async Task When_No_Valid_Parameters_Is_Passed_Then_Error_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>> { new("invalid", "invalid") };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/token/revoke")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest);
        var json = await httpResult.Content.ReadAsStringAsync();

        var error = JsonConvert.DeserializeObject<ErrorDetails>(json)!;

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
        var ex = Assert.IsType<Option.Error>(
            await tokenClient.RevokeToken(RevokeTokenRequest.Create("access_token", TokenTypes.AccessToken)));

        Assert.Equal("invalid_client", ex.Details.Title);
        Assert.Equal(SharedStrings.TheClientDoesntExist, ex.Details.Detail);
    }

    [Fact]
    public async Task When_Token_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var ex = Assert.IsType<Option.Error>(
            await tokenClient.RevokeToken(RevokeTokenRequest.Create("access_token", TokenTypes.AccessToken)));

        Assert.Equal("invalid_token", ex.Details.Title);
        Assert.Equal(Strings.TheTokenDoesntExist, ex.Details.Detail);
    }

    [Fact]
    public async Task When_Revoke_Token_And_Client_Is_Different_Then_Error_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
        );
        var revokeClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var ex = Assert.IsType<Option.Error>(await revokeClient
            .RevokeToken(RevokeTokenRequest.Create(result.Item.AccessToken, TokenTypes.AccessToken)));

        Assert.Equal("invalid_token", ex.Details.Title);
        Assert.Equal("The token has not been issued for the given client id 'client'", ex.Details.Detail);
    }

    [Fact]
    public async Task When_Revoking_AccessToken_Then_True_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
        );
        var revoke = await tokenClient
                .RevokeToken(RevokeTokenRequest.Create(result.Item.AccessToken, TokenTypes.AccessToken))
            as Option.Success;
        var introspectionClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var ex = await introspectionClient.Introspect(
            IntrospectionRequest.Create(result.Item.AccessToken, TokenTypes.AccessToken, "pat"));

        Assert.IsType<Option.Success>(revoke);
        Assert.IsType<Option<UmaIntrospectionResponse>.Error>(ex);
    }

    [Fact]
    public async Task When_Revoking_RefreshToken_Then_True_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim", "offline" }))
        );
        var revoke = await tokenClient
            .RevokeToken(RevokeTokenRequest.Create(result.Item.RefreshToken!, TokenTypes.RefreshToken));
        var introspectClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var ex = await introspectClient.Introspect(
            IntrospectionRequest.Create(result.Item.RefreshToken!, TokenTypes.RefreshToken, "pat"));

        Assert.IsType<Option.Success>(revoke);
        Assert.IsType<Option<UmaIntrospectionResponse>.Error>(ex);
    }
}
