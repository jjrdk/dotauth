namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Requests;
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
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);
                });

            "Then resource owner is local account".x(
                async () =>
                {
                    var response = await _managerClient.GetResourceOwner("test", _grantedToken.AccessToken)
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
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);
                });

            "Then can update resource owner password".x(
                async () =>
                {
                    var response = await _managerClient.UpdateResourceOwnerPassword(
                            new UpdateResourceOwnerPasswordRequest { Subject = "test", Password = "test2" },
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);
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
                        Claims = new[] {new PostClaim {Type = "test", Value = "something"}}
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _grantedToken.AccessToken);
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is ok request".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                });
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
                        Claims = new[] {new PostClaim {Type = "test", Value = "something"}}
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _grantedToken.AccessToken);
                    response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "Then is bad request".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                });
        }
    }
}
