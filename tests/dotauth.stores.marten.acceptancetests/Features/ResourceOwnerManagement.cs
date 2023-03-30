namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private GrantedTokenResponse _updatedToken = null!;
    private GrantedTokenResponse _newToken = null!;
    
    [When(@"adding resource owner with (.+), (.+)")]
    public async Task WhenAddingResourceOwnerWith(string subject, string password)
    {
        var response = await _managerClient.AddResourceOwner(
                new AddResourceOwnerRequest {Password = password, Subject = subject},
                _token.AccessToken)
            .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;

        Assert.NotNull(response);
    }
    
    [Then(@"resource owner (.+) is local account")]
    public async Task ThenResourceOwnerIsLocalAccount(string subject)
    {
        var response = await _managerClient.GetResourceOwner(subject, _token.AccessToken)
            .ConfigureAwait(false) as Option<ResourceOwner>.Result;

        Assert.True(response!.Item.IsLocalAccount);
    }

    [Then(@"can update resource owner (.+) with password (.+)")]
    public async Task ThenCanUpdateResourceOwnerWithPassword(string subject, string password)
    {
        var response = await _managerClient.UpdateResourceOwnerPassword(
                new UpdateResourceOwnerPasswordRequest {Subject = subject, Password = password},
                _token.AccessToken)
            .ConfigureAwait(false);

        Assert.IsType<Option.Success>(response);
    }

    [Then(@"user can login (.+) with new password (.+)")]
    public async Task ThenUserCanLoginWithNewPassword(string subject, string password)
    {
        var option =
            await _tokenClient.GetToken(TokenRequest.FromPassword(subject, password, new[] {"manager"}))
                .ConfigureAwait(false);
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(result.Item);
    }

    [When(@"updating user (.+) claims")]
    public async Task WhenUpdatingUserClaims(string subject, Table claims)
    {
        var claimData = claims.Rows.Select(row => new ClaimData { Type = row["Type"], Value = row["Value"] }).ToArray();
        var updateRequest = new UpdateResourceOwnerClaimsRequest
        {
            Subject = subject,
            Claims = claimData
        };

        var json = JsonConvert.SerializeObject(updateRequest);

        var request = new HttpRequestMessage
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            Method = HttpMethod.Post,
            RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _token.AccessToken);
        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }

    [Then(@"is ok request")]
    public void ThenIsOkRequest()
    {
        Assert.Equal(HttpStatusCode.OK, _responseMessage.StatusCode);
    }

    [Then(@"has new token")]
    public async Task ThenHasNewToken()
    {
        var json = await _responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

        _updatedToken = JsonConvert.DeserializeObject<GrantedTokenResponse>(json)!;

        Assert.NotNull(_updatedToken);
    }

    [Then(@"has new admin token")]
    public async Task ThenHasNewAdminToken()
    {
        var json = await _responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

        _token = JsonConvert.DeserializeObject<GrantedTokenResponse>(json)!;

        Assert.NotNull(_token);
    }

    [When(@"refreshing token, then has updated claims")]
    public async Task WhenRefreshingTokenThenHasUpdatedClaims()
    {
        var option = await _tokenClient
            .GetToken(TokenRequest.FromRefreshToken(_token.RefreshToken!))
            .ConfigureAwait(false);
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(result!.Item);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(result.Item.AccessToken) as JwtSecurityToken;
        Assert.Contains(token!.Claims, c => c.Type == "added_claim_test" && c.Value == "something");
    }

    [When(@"revoking token")]
    public async Task WhenRevokingToken()
    {
        var result = await _tokenClient.RevokeToken(RevokeTokenRequest.Create(_updatedToken))
            .ConfigureAwait(false);
        Assert.IsType<Option.Success>(result);
    }

    [When(@"logging in again")]
    public async Task WhenLoggingInAgain()
    {
        var option = await _tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "manager" }))
            .ConfigureAwait(false);

        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        _newToken = result.Item;
    }

    [Then(@"gets updated claim in token")]
    public void ThenGetsUpdatedClaimInToken()
    {
        var handler = new JwtSecurityTokenHandler();
        var updatedJwt = handler.ReadJwtToken(_updatedToken.AccessToken);
        var newJwt = handler.ReadJwtToken(_newToken.AccessToken);

        Assert.Equal(
            updatedJwt.Claims.First(x => x.Type == "added_claim_test").Value,
            newJwt.Claims.First(x => x.Type == "added_claim_test").Value);
    }

    [Then(@"resource owner has new claim")]
    public async Task ThenResourceOwnerHasNewClaim()
    {
        var option =
            await _tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] {"manager", "offline"}))
                .ConfigureAwait(false);
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(result.Item);

        var handler = new JwtSecurityTokenHandler();
        var token = (JwtSecurityToken) handler.ReadToken(result.Item.AccessToken);
        Assert.Contains(token.Claims, c => c.Type == "added_claim_test" && c.Value == "something");
    }

    [When(@"deleting user claims")]
    public async Task WhenDeletingUserClaims()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(
                _fixture.Server.BaseAddress + "resource_owners/claims?type=acceptance_test")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _token.AccessToken);
        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }

    [Then(@"resource owner no longer has claim")]
    public async Task ThenResourceOwnerNoLongerHasClaim()
    {
        var option =
            await _tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"manager"}))
                .ConfigureAwait(false);
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(result.Item);

        var handler = new JwtSecurityTokenHandler();
        var token = (JwtSecurityToken) handler.ReadToken(result.Item.AccessToken);
        Assert.DoesNotContain(token.Claims, c => c.Type == "acceptance_test");
    }

    [When(@"deleting user claims not in scope")]
    public async Task WhenDeletingUserClaimsNotInScope()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims?type=some_other_claim")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }


    private ResourceOwner _resourceOwner = null!;
    
    [Then(@"when getting resource owner from store")]
    public async Task ThenWhenGettingResourceOwnerFromStore()
    {
        var store = (IResourceOwnerStore) _fixture.Server.Host.Services.GetRequiredService(
            typeof(IResourceOwnerStore));
        _resourceOwner = (await store.Get("administrator", CancellationToken.None).ConfigureAwait(false))!;
    }

    [Then(@"resource owner still has claim")]
    public void ThenResourceOwnerStillHasClaim()
    {
        Assert.Contains(_resourceOwner.Claims, c => c.Type == "some_other_claim");
    }

    [Then(@"is bad request")]
    public void ThenIsBadRequest()
    {
        Assert.Equal(HttpStatusCode.BadRequest, _responseMessage.StatusCode);
    }

    [When(@"deleting own account")]
    public async Task WhenDeletingOwnAccount()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete, RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }

    [When(@"putting user claims")]
    public async Task WhenPuttingUserClaims()
    {
        var updateRequest = new UpdateResourceOwnerClaimsRequest
        {
            Subject = "user",
            Claims = new[]
            {
                new ClaimData {Type = OpenIdClaimTypes.Subject, Value = "user"},
                new ClaimData {Type = OpenIdClaimTypes.Name, Value = "John Doe"},
                new ClaimData {Type = "acceptance_test", Value = "test"},
                new ClaimData {Type = "test", Value = "something"}
            }
        };

        var json = JsonConvert.SerializeObject(updateRequest);

        var request = new HttpRequestMessage
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            Method = HttpMethod.Put,
            RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _token.AccessToken);
        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }

    [Given(@"an manager token")]
    public async Task GivenAnManagerToken()
    {
        var option = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", new[] {"manager", "offline"}))
            .ConfigureAwait(false);
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(result.Item);

        _token = result.Item;
    }
}