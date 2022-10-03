namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

// In order to manage resource owners in the system
// As a manager
// I want to perform management actions on resource owners.
public sealed class ResourceOwnerManagementFeature : AuthorizedManagementFeatureBase
{
    /// <inheritdoc />
    public ResourceOwnerManagementFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario]
    public void SuccessAddResourceOwner()
    {
        "When adding resource owner".x(
            async () =>
            {
                var response = await _managerClient.AddResourceOwner(
                        new AddResourceOwnerRequest {Password = "test", Subject = "test"},
                        _administratorToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;

                Assert.NotNull(response);
            });

        "Then resource owner is local account".x(
            async () =>
            {
                var response = await _managerClient.GetResourceOwner("test", _administratorToken.AccessToken)
                    .ConfigureAwait(false) as Option<ResourceOwner>.Result;

                Assert.True(response.Item.IsLocalAccount);
            });
    }

    [Scenario]
    public void SuccessUpdateResourceOwnerPassword()
    {
        "When adding resource owner".x(
            async () =>
            {
                var response = await _managerClient.AddResourceOwner(
                        new AddResourceOwnerRequest {Password = "test", Subject = "test"},
                        _administratorToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;

                Assert.NotNull(response);
            });

        "Then can update resource owner password".x(
            async () =>
            {
                var response = await _managerClient.UpdateResourceOwnerPassword(
                        new UpdateResourceOwnerPasswordRequest {Subject = "test", Password = "test2"},
                        _administratorToken.AccessToken)
                    .ConfigureAwait(false);

                Assert.NotNull(response as Option.Success);
            });

        "Then user can login with new password".x(
            async () =>
            {
                var result =
                    await _tokenClient.GetToken(TokenRequest.FromPassword("test", "test2", new[] {"manager"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                Assert.NotNull(result.Item);
            });
    }

    [Scenario]
    public void CanUpdateOwnClaimsAndRefresh()
    {
        HttpResponseMessage response = null!;

        "When updating user claims".x(
            async () =>
            {
                var updateRequest = new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "administrator",
                    Claims = new[] {new ClaimData {Type = "added_claim_test", Value = "something"}}
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
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is ok request".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });

        "and has new token".x(
            async () =>
            {
                var updatedToken = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Assert.NotNull(updatedToken);
            });

        "When refreshing token, then has updated claims".x(
            async () =>
            {
                var result = await _tokenClient
                    .GetToken(TokenRequest.FromRefreshToken(_administratorToken.RefreshToken))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                Assert.NotNull(result.Item);

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(result.Item.AccessToken) as JwtSecurityToken;
                Assert.Contains(token.Claims, c => c.Type == "added_claim_test" && c.Value == "something");
            });
    }

    [Scenario]
    public void CanUpdateOwnClaimsAndLogInAgain()
    {
        HttpResponseMessage response = null!;
        GrantedTokenResponse updatedToken = null!;
        GrantedTokenResponse newToken = null!;

        "When updating user claims".x(
            async () =>
            {
                var updateRequest = new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "administrator",
                    Claims = new[] {new ClaimData {Type = "added_claim_test", Value = "something"}}
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
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is ok request".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });

        "and has new token".x(
            async () =>
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                updatedToken = JsonConvert.DeserializeObject<GrantedTokenResponse>(json);

                Assert.NotNull(updatedToken);
            });

        "When logging out".x(
            async () =>
            {
                var result = await _tokenClient.RevokeToken(RevokeTokenRequest.Create(updatedToken))
                    .ConfigureAwait(false);
                Assert.IsType<Option.Success>(result);
            });

        "and logging in again".x(
            async () =>
            {
                var result = await _tokenClient
                    .GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"manager"}))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(result);

                newToken = result.Item;
            });

        "then gets updated claim in token".x(
            () =>
            {
                var handler = new JwtSecurityTokenHandler();
                var updatedJwt = handler.ReadJwtToken(updatedToken.AccessToken);
                var newJwt = handler.ReadJwtToken(newToken.AccessToken);

                Assert.Equal(
                    updatedJwt.Claims.First(x => x.Type == "added_claim_test").Value,
                    newJwt.Claims.First(x => x.Type == "added_claim_test").Value);
            });
    }

    [Scenario]
    public void CanUpdateOwnClaims()
    {
        HttpResponseMessage response = null!;

        "When updating user claims".x(
            async () =>
            {
                var updateRequest = new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "administrator",
                    Claims = new[] {new ClaimData {Type = "added_claim_test", Value = "something"}}
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
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is ok request".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });

        "Then resource owner has new claim".x(
            async () =>
            {
                var result =
                    await _tokenClient.GetToken(
                            TokenRequest.FromPassword("administrator", "password", new[] {"manager", "offline"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(result.Item);

                var handler = new JwtSecurityTokenHandler();
                var token = (JwtSecurityToken) handler.ReadToken(result.Item.AccessToken);
                Assert.Contains(token.Claims, c => c.Type == "added_claim_test" && c.Value == "something");
            });
    }

    [Scenario]
    public void CanDeleteOwnClaims()
    {
        HttpResponseMessage response = null!;

        "When deleting user claims".x(
            async () =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(
                        _fixture.Server.BaseAddress + "resource_owners/claims?type=acceptance_test")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is ok request".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });

        "Then resource owner no longer has claim".x(
            async () =>
            {
                var result =
                    await _tokenClient
                        .GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"manager"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(result.Item);

                var handler = new JwtSecurityTokenHandler();
                var token = (JwtSecurityToken) handler.ReadToken(result.Item.AccessToken);
                Assert.DoesNotContain(token.Claims, c => c.Type == "acceptance_test");
            });
    }

    [Scenario]
    public void CannotDeleteClaimsNotPartOfScope()
    {
        HttpResponseMessage response = null!;
        ResourceOwner resourceOwner = null!;

        "When deleting user claims not in scope".x(
            async () =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(
                        _fixture.Server.BaseAddress + "resource_owners/claims?type=some_other_claim")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is ok request".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });

        "And when getting resource owner from store".x(
            async () =>
            {
                var store = (IResourceOwnerStore) _fixture.Server.Host.Services.GetService(
                    typeof(IResourceOwnerStore));
                resourceOwner = await store.Get("administrator", CancellationToken.None).ConfigureAwait(false);
            });

        "Then resource owner still has claim".x(
            () => { Assert.Contains(resourceOwner.Claims, c => c.Type == "some_other_claim"); });
    }

    [Scenario]
    public void CanUpdateOwnClaimsTwoTimes()
    {
        HttpResponseMessage response = null!;

        "When updating user claims".x(
            async () =>
            {
                var updateRequest = new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "administrator",
                    Claims = new[] {new ClaimData {Type = "added_claim_test", Value = "something"}}
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
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is ok response".x(
            async () =>
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _administratorToken = JsonConvert.DeserializeObject<GrantedTokenResponse>(json);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });

        "and when updating second time".x(
            async () =>
            {
                var updateRequest = new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "administrator",
                    Claims = new[] {new ClaimData {Type = "added_claim_test2", Value = "something"}}
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
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is also ok response".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });
    }

    [Scenario]
    public void CannotUpdateOtherUsersClaims()
    {
        HttpResponseMessage response = null!;

        "When updating user claims".x(
            async () =>
            {
                var updateRequest = new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "user", Claims = new[] {new ClaimData {Type = "test", Value = "something"}}
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
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is bad request".x(() => { Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); });
    }

    [Scenario]
    public void CanDeleteOwnAccount()
    {
        HttpResponseMessage response = null!;

        "When deleting own account".x(
            async () =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then response is OK".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });
    }

    [Scenario]
    public void AdministratorsCanUpdateOtherUsersClaims()
    {
        HttpResponseMessage response = null!;

        "When updating user claims".x(
            async () =>
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
                    _administratorToken.AccessToken);
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "Then is ok request".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });
    }
}