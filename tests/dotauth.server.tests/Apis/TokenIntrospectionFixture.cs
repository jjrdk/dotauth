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

namespace DotAuth.Server.Tests.Apis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

public sealed class TokenIntrospectionFixture
{
    private const string BaseUrl = "http://localhost:5000";
    private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
    private readonly TestOauthServerFixture _server;

    public TokenIntrospectionFixture(ITestOutputHelper outputHelper)
    {
        _server = new TestOauthServerFixture(outputHelper);
    }

    [Fact]
    public async Task When_No_Parameters_Is_Passed_To_Introspection_Edp_Then_Error_Is_Returned()
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, RequestUri = new Uri($"{BaseUrl}/introspect")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest);
        var json = await httpResult.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        var error = JsonConvert.DeserializeObject<ErrorDetails>(json)!;
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal("no parameter in body request", error.Detail);
    }

    [Fact]
    public async Task When_No_Valid_Parameters_Is_Passed_Then_Error_Is_Returned()
    {
        var request = new List<KeyValuePair<string, string>> { new("invalid", "invalid") };
        var body = new FormUrlEncodedContent(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, Content = body, RequestUri = new Uri($"{BaseUrl}/introspect")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest);
        var json = await httpResult.Content.ReadAsStringAsync();

        var error = JsonConvert.DeserializeObject<ErrorDetails>(json)!;
        Assert.Equal(ErrorCodes.InvalidRequest, error.Title);
        Assert.Equal("no parameter in body request", error.Detail);
    }

    [Fact]
    public async Task WhenIntrospectingAndTokenDoesNotExistThenResponseShowsInactiveToken()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var introspection = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await tokenClient.Introspect(
                IntrospectionRequest.Create("invalid_token", TokenTypes.AccessToken, "pat"))
            );

        Assert.False(introspection.Item.Active);
    }

    [Fact]
    public async Task When_Introspecting_AccessToken_Then_Information_Are_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(
            await tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                );
        var introspection = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await tokenClient.Introspect(
                IntrospectionRequest.Create(result.Item.AccessToken, TokenTypes.AccessToken, "pat"))
            );

        Assert.Single(introspection.Item.Scope);
        Assert.Equal("scim", introspection.Item.Scope.First());
    }

    [Fact]
    public async Task When_Introspecting_RefreshToken_Then_Information_Are_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", new[] { "scim", "offline" }))
            );

        var introspection = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await tokenClient.Introspect(
                IntrospectionRequest.Create(result.Item.RefreshToken!, TokenTypes.RefreshToken, "pat"))
            );

        Assert.Equal(2, introspection.Item.Scope.Length);
        Assert.Equal("scim", introspection.Item.Scope.First());
    }
}
