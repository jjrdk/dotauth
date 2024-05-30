namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    [When(@"getting a token for (.+), (.+) for scope (.+)")]
    public async Task WhenGettingATokenForForScope(string user, string password, string scope)
    {
        var scopes = scope.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var option = await _tokenClient.GetToken(TokenRequest.FromPassword(user, password, scopes))
            .ConfigureAwait(false);

        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        _token = response.Item;
    }

    [When(@"getting a token option for (.+), (.+) for scope (.+)")]
    public async Task WhenGettingATokenOptionForForScope(string user, string password, string scope)
    {
        var scopes = scope.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        _tokenOption = await _tokenClient.GetToken(TokenRequest.FromPassword(user, password, scopes))
            .ConfigureAwait(false);
    }

    [Then(@"has valid access token for audience (.+)")]
    public void ThenHasValidAccessTokenForAudience(string audience)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = _serverKeySet.GetSigningKeys(),
            ValidAudience = "client",
            ValidIssuer = "https://localhost"
        };
        tokenHandler.ValidateToken(_token.AccessToken, validationParameters, out var token);

        Assert.NotEmpty(((JwtSecurityToken)token).Claims);
    }

    [Then(@"has valid id token for audience (.+)")]
    public void ThenHasValidIdTokenForAudience(string client)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = TestKeys.SecretKey.CreateJwk(
                JsonWebKeyUseNames.Sig,
                KeyOperations.Sign,
                KeyOperations.Verify),
            ValidAudience = client,
            ValidIssuer = "https://localhost"
        };
        tokenHandler.ValidateToken(_token.IdToken, validationParameters, out _);
    }

    [When(@"has valid access token for audience (.+)")]
    public void WhenHasValidAccessTokenForAudienceClient(string client)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = _serverKeySet.GetSigningKeys()[0],
            ValidAudience = client,
            ValidIssuer = "https://localhost"
        };
        tokenHandler.ValidateToken(_token.AccessToken, validationParameters, out _);
    }

    [Then(@"can get user info response")]
    public async Task ThenCanGetUserInfoResponse()
    {
        var userinfoRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = new Uri($"{BaseUrl}/userinfo")
        };
        userinfoRequest.Headers.Authorization =
            new AuthenticationHeaderValue(_token.TokenType, _token.AccessToken);
        var userinfo = await _fixture.Client().SendAsync(userinfoRequest).ConfigureAwait(false);

        Assert.True(userinfo.IsSuccessStatusCode);
    }

    [When(@"updating own claims")]
    public async Task WhenUpdatingOwnClaims()
    {
        var updateRequest = new UpdateResourceOwnerClaimsRequest
        {
            Subject = "user", Claims = [new ClaimData {Type = "test", Value = "something"}]
        };

        var json = JsonConvert.SerializeObject(updateRequest);

        var request = new HttpRequestMessage
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            Method = HttpMethod.Post,
            RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            _token.AccessToken);
        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }

    [Then(@"update is successful")]
    public void ThenUpdateIsSuccessful()
    {
        Assert.Equal(HttpStatusCode.OK, _responseMessage.StatusCode);
    }

    [Then(@"token has custom custom claims")]
    public void ThenTokenHasCustomCustomClaims()
    {
        var handler = new JwtSecurityTokenHandler();
        var refreshedClaims = handler.ReadJwtToken(_token.AccessToken).Claims;

        Assert.Contains(refreshedClaims, c => c.Type == "acceptance_test");
    }
}
