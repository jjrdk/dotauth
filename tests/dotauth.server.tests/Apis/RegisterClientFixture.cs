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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using Xunit;
using Xunit.Abstractions;

public sealed class RegisterClientFixture : IDisposable
{
    private const string BaseUrl = "http://localhost:5000";
    private const string ApplicationJson = "application/json";
    private readonly TestOauthServerFixture _server;

    public RegisterClientFixture(ITestOutputHelper outputHelper)
    {
        _server = new TestOauthServerFixture(outputHelper);
    }

    [Fact(Skip = "Run locally")]
    public async Task When_Empty_Json_Request_Is_Passed_To_Registration_Api_Then_Error_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri($"{BaseUrl}/.well-known/openid-configuration"));
        var grantedToken = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromScopes("register_client"))
        );
        var obj = new { fake = "fake" };
        var fakeJson = JsonSerializer.Serialize(
            obj,
            SharedSerializerContext.Default.Options);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{BaseUrl}/clients"),
            Content = new StringContent(fakeJson)
        };
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", grantedToken.Item.AccessToken);

        var httpResult = await _server.Client().SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
    }

    [Fact]
    public async Task WhenPassInvalidRedirectUrisThenErrorIsReturned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri($"{BaseUrl}/.well-known/openid-configuration"));
        var grantedToken = Assert.IsType<Option<GrantedTokenResponse>.Result>(
            await tokenClient.GetToken(TokenRequest.FromScopes("manager")));
        var obj = new
        {
            allowed_scopes = new[] { "openid" },
            request_uris = new[] { new Uri("https://localhost") },
            redirect_uris = new[] { "localhost" },
            client_uri = new Uri("http://google.com"),
            tos_uri = new Uri("http://google.com"),
            jwks = TestKeys.SecretKey.CreateSignatureJwk().ToSet()
        };
        var fakeJson = JsonSerializer.Serialize(obj);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{BaseUrl}/clients"),
            Content = new StringContent(fakeJson)
        };
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", grantedToken.Item.AccessToken);

        var httpResult = await _server.Client().SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
        var json = await httpResult.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize(json, SharedSerializerContext.Default.ErrorDetails);

        Assert.Equal(ErrorCodes.InvalidRedirectUri, error!.Title);
    }

    [Fact(Skip = "Run locally")]
    public async Task When_Pass_Redirect_Uri_With_Fragment_Then_Error_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri($"{BaseUrl}/.well-known/openid-configuration"));
        var grantedToken = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromScopes("register_client")));
        var obj = new
        {
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            AllowedScopes = new[] { "openid" },
            RequestUris = new[] { new Uri("https://localhost") },
            RedirectionUrls = new[] { new Uri("http://localhost#fragment") },
            //LogoUri = "http://google.com",
            ClientUri = new Uri("https://valid")
        };
        var fakeJson = JsonSerializer.Serialize(
            obj, SharedSerializerContext.Default.Options);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{BaseUrl}/registration"),
            Content = new StringContent(fakeJson)
        };
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(ApplicationJson);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            grantedToken.Item.TokenType,
            grantedToken.Item.AccessToken);

        var httpResult = await _server.SharedCtx.Client().SendAsync(httpRequest);

        //Assert.Equal(HttpStatusCode.OK, httpResult.StatusCode);

        var json = await httpResult.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize(json, SharedSerializerContext.Default.ErrorDetails);

        Assert.Equal("invalid_redirect_uri", error!.Title);
        Assert.Equal(
            string.Format(Strings.TheRedirectUrlCannotContainsFragment, "http://localhost/#fragment"),
            error.Detail);
    }

    [Fact(Skip = "Run locally")]
    public async Task When_Pass_Invalid_Client_Uri_Then_Error_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri($"{BaseUrl}/.well-known/openid-configuration"));
        var grantedToken = Assert.IsType<Option<GrantedTokenResponse>.Result>(
            await tokenClient.GetToken(TokenRequest.FromScopes("register_client")));
        var obj = new
        {
            AllowedScopes = new[] { "openid" },
            RequestUris = new[] { new Uri("https://localhost") },
            RedirectionUrls = new[] { new Uri("http://localhost") },
            LogoUri = "http://google.com",
            ClientUri = "invalid_client_uri"
        };
        var fakeJson = JsonSerializer.Serialize(
            obj, SharedSerializerContext.Default.Options);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{BaseUrl}/registration"),
            Content = new StringContent(fakeJson, Encoding.UTF8, ApplicationJson)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", grantedToken.Item.AccessToken);

        var httpResult = await _server.Client().SendAsync(httpRequest);
        var json = await httpResult.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize(json, SharedSerializerContext.Default.ErrorDetails);

        Assert.Equal("invalid_client_metadata", error!.Title);
        Assert.Equal("the parameter client_uri is not correct", error.Detail);
    }

    [Fact(Skip = "Run locally")]
    public async Task When_Pass_Invalid_Tos_Uri_Then_Error_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri($"{BaseUrl}/.well-known/openid-configuration"));
        var grantedToken = Assert.IsType<Option<GrantedTokenResponse>.Result>(
            await tokenClient.GetToken(TokenRequest.FromScopes("register_client")));
        var obj = new
        {
            AllowedScopes = new[] { "openid" },
            RequestUris = new[] { new Uri("https://localhost") },
            RedirectionUrls = new[] { new Uri("http://localhost") },
            LogoUri = new Uri("http://google.com"),
            ClientUri = new Uri("https://valid_client_uri"),
            TosUri = "invalid"
        };
        var fakeJson = JsonSerializer.Serialize(
            obj, SharedSerializerContext.Default.Options);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{BaseUrl}/registration"),
            Content = new StringContent(fakeJson)
        };
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", grantedToken.Item.AccessToken);

        var httpResult = await _server.Client().SendAsync(httpRequest);
        var json = await httpResult.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize(json, SharedSerializerContext.Default.ErrorDetails);

        Assert.Equal("invalid_client_metadata", error!.Title);
        Assert.Equal("the parameter tos_uri is not correct", error.Detail);
    }

    [Fact]
    public async Task When_Registering_A_Client_Then_No_Exception_Is_Thrown()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri($"{BaseUrl}/.well-known/openid-configuration"));
        var grantedToken = Assert.IsType<Option<GrantedTokenResponse>.Result>(
            await tokenClient.GetToken(TokenRequest.FromScopes("manager")));

        var registrationClient = await ManagementClient.Create(
            _server.Client,
            new Uri($"{BaseUrl}/.well-known/openid-configuration"));
        var register = await registrationClient.Register(
            new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                AllowedScopes = ["openid"],
                ClientName = "Test",
                ClientId = "id",
                RedirectionUrls = [new Uri("https://localhost")],
            },
            grantedToken.Item.AccessToken);
        var client = Assert.IsType<Option<Client>.Result>(register);

        Assert.NotNull(client);
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
