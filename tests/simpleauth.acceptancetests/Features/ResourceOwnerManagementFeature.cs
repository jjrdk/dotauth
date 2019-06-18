namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Newtonsoft.Json;
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    // In order to manage resource owners in the system
    // As a manager
    // I want to perform management actions on resource owners.
    public class ResourceOwnerManagementFeature : AuthorizedManagementFeatureBase
    {
        [Scenario]
        public void SuccessAddResourceOwner()
        {
            "When adding resource owner".x(
                async () =>
                {
                    var response = await _managerClient.AddResourceOwner(
                            new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                            _administratorToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);
                });

            "Then resource owner is local account".x(
                async () =>
                {
                    var response = await _managerClient.GetResourceOwner("test", _administratorToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.True(response.Content.IsLocalAccount);
                });
        }

        [Scenario]
        public void SuccessUpdateResourceOwnerPassword()
        {
            "When adding resource owner".x(
                async () =>
                {
                    var response = await _managerClient.AddResourceOwner(
                            new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                            _administratorToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);
                });

            "Then can update resource owner password".x(
                async () =>
                {
                    var response = await _managerClient.UpdateResourceOwnerPassword(
                            new UpdateResourceOwnerPasswordRequest { Subject = "test", Password = "test2" },
                            _administratorToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);
                });

            "Then user can login with new password".x(
                async () =>
                {
                    var result = await _tokenClient.GetToken(TokenRequest.FromPassword("test", "test2", new[] { "manager" })).ConfigureAwait(false);
                    Assert.NotNull(result.Content);
                });
        }

        [Scenario]
        public void CanUpdateOwnClaimsAndRefresh()
        {
            HttpResponseMessage response = null;

            "When updating user claims".x(
                async () =>
                {
                    var updateRequest = new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "administrator",
                        Claims = new[] { new PostClaim { Type = "added_claim_test", Value = "something" } }
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
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is ok request".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                });

            "and has new token".x(async () =>
            {
                var updatedToken = await response.Content.ReadAsStringAsync();
                Assert.NotNull(updatedToken);
            });

            "When refreshing token, then has updated claims".x(
                async () =>
                {
                    var result = await _tokenClient
                        .GetToken(TokenRequest.FromRefreshToken(_administratorToken.RefreshToken))
                        .ConfigureAwait(false);
                    Assert.NotNull(result.Content);

                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadToken(result.Content.AccessToken) as JwtSecurityToken;
                    Assert.Contains(token.Claims, c => c.Type == "added_claim_test" && c.Value == "something");
                });
        }

        [Scenario]
        public void CanUpdateOwnClaims()
        {
            HttpResponseMessage response = null;

            "When updating user claims".x(
                async () =>
                {
                    var updateRequest = new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "administrator",
                        Claims = new[] { new PostClaim { Type = "added_claim_test", Value = "something" } }
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _administratorToken.AccessToken);
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is ok request".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                });

            "Then resource owner has new claim".x(
                async () =>
                {
                    var result = await _tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "manager" })).ConfigureAwait(false);
                    Assert.NotNull(result.Content);

                    var handler = new JwtSecurityTokenHandler();
                    var token = (JwtSecurityToken)handler.ReadToken(result.Content.AccessToken);
                    Assert.Contains(token.Claims, c => c.Type == "added_claim_test" && c.Value == "something");
                });
        }

        [Scenario]
        public void CanDeleteOwnClaims()
        {
            HttpResponseMessage response = null;

            "When deleting user claims".x(
                async () =>
                {
                    var updateRequest = new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "administrator",
                        Claims = new[] { new PostClaim { Type = "acceptance_test" } }
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _administratorToken.AccessToken);
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is ok request".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                });

            "Then resource owner no longer has claim".x(
                async () =>
                {
                    var result = await _tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "manager" })).ConfigureAwait(false);
                    Assert.NotNull(result.Content);

                    var handler = new JwtSecurityTokenHandler();
                    var token = (JwtSecurityToken)handler.ReadToken(result.Content.AccessToken);
                    Assert.DoesNotContain(token.Claims, c => c.Type == "acceptance_test");
                });
        }

        [Scenario]
        public void CanUpdateOwnClaimsTwoTimes()
        {
            HttpResponseMessage response = null;

            "When updating user claims".x(
                async () =>
                {
                    var updateRequest = new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "administrator",
                        Claims = new[] { new PostClaim { Type = "added_claim_test", Value = "something" } }
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", _administratorToken.AccessToken);
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is ok response".x(
                async () =>
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _administratorToken = JsonConvert.DeserializeObject<GrantedTokenResponse>(json);

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                });

            "and when updating second time".x(async () =>
            {
                var updateRequest = new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "administrator",
                    Claims = new[] { new PostClaim { Type = "added_claim_test2", Value = "something" } }
                };

                var json = JsonConvert.SerializeObject(updateRequest);

                var request = new HttpRequestMessage
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                };
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _administratorToken.AccessToken);
                response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
            });

            "Then is also ok response".x(
                () => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });
        }

        [Scenario]
        public void CannotUpdateOtherUsersClaims()
        {
            HttpResponseMessage response = null;

            "When updating user claims".x(
                async () =>
                {
                    var updateRequest = new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "user",
                        Claims = new[] { new PostClaim { Type = "test", Value = "something" } }
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _administratorToken.AccessToken);
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is bad request".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                });
        }

        [Scenario]
        public void AdministratorsCanUpdateOtherUsersClaims()
        {
            HttpResponseMessage response = null;

            "When updating user claims".x(
                async () =>
                {
                    var updateRequest = new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "user",
                        Claims = new[]
                        {
                            new PostClaim {Type = OpenIdClaimTypes.Subject, Value = "user"},
                            new PostClaim {Type = OpenIdClaimTypes.Name, Value = "John Doe"},
                            new PostClaim {Type = "acceptance_test", Value = "test"},
                            new PostClaim {Type = "test", Value = "something"}
                        }
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Put,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _administratorToken.AccessToken);
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is ok request".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                });
        }
    }
}
